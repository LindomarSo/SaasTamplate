using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;
using Stripe;
using Stripe.Checkout;

namespace SaasStarter.Infra.Services;

public class StripeService : IStripeService
{
    private readonly string _webhookSecret;
    private readonly string _frontendBaseUrl;
    private readonly List<string> _paymentMethodTypes;
    private readonly ILogger<StripeService> _logger;

    // TODO: substitua o nome do produto pelo nome do seu SaaS
    private const string ProductName = "SaasStarter";

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _logger = logger;

        var secretKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey não configurado.");
        _webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Stripe:WebhookSecret não configurado.");
        _frontendBaseUrl = configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl não configurado.");

        var configured = configuration.GetSection("Stripe:PaymentMethods").Get<List<string>>();
        _paymentMethodTypes = configured is { Count: > 0 } ? configured : ["card", "boleto"];

        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid userId,
        string userEmail,
        PlanDefinition plan,
        decimal? discountedPrice = null,
        CancellationToken cancellationToken = default)
    {
        var effectivePrice = discountedPrice ?? plan.Price;
        var unitAmountInCents = (long)Math.Round(effectivePrice * 100, MidpointRounding.AwayFromZero);

        var hasBoleto = _paymentMethodTypes.Contains("boleto");
        var hasPix = _paymentMethodTypes.Contains("pix");

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = _paymentMethodTypes,
            Mode = "payment",
            CustomerEmail = userEmail,
            CustomerCreation = hasBoleto ? "always" : "if_required",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "brl",
                        UnitAmount = unitAmountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            // TODO: personalize a descrição do produto
                            Name = $"{ProductName} — Plano {plan.Name}",
                            Description = plan.Description
                        }
                    }
                }
            ],
            PaymentMethodOptions = new SessionPaymentMethodOptionsOptions
            {
                Boleto = hasBoleto ? new SessionPaymentMethodOptionsBoletoOptions { ExpiresAfterDays = 3 } : null,
                Pix = hasPix ? new SessionPaymentMethodOptionsPixOptions { ExpiresAfterSeconds = 3600 } : null
            },
            // IMPORTANTE: acesso liberado APENAS via webhook, nunca pela success_url
            SuccessUrl = $"{_frontendBaseUrl}/pagamento/sucesso",
            CancelUrl = $"{_frontendBaseUrl}/planos",
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "planDefinitionId", plan.Id.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogDebug("Stripe Session criada. SessionId: {SessionId}", session.Id);

        return new StripeCheckoutSessionResult(session.Id, session.Url);
    }

    public StripeWebhookEvent ParseWebhookEvent(string payload, string stripeSignature)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            stripeSignature,
            _webhookSecret,
            throwOnApiVersionMismatch: false);

        if (stripeEvent.Data.Object is not Session session)
            throw new InvalidOperationException($"Tipo de objeto inesperado no evento '{stripeEvent.Type}'.");

        return new StripeWebhookEvent(
            Type: stripeEvent.Type,
            SessionId: session.Id,
            PaymentIntentId: session.PaymentIntentId,
            CustomerEmail: session.CustomerEmail,
            AmountTotal: session.AmountTotal ?? 0,
            PaymentStatus: session.PaymentStatus ?? "unpaid",
            Metadata: session.Metadata ?? new Dictionary<string, string>());
    }
}
