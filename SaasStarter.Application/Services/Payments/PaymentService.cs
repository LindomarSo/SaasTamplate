using Microsoft.Extensions.Logging;
using SaasStarter.Application.Common;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IStripeService _stripeService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IPromotionCodeRepository _promotionCodeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IStripeService stripeService,
        IPaymentRepository paymentRepository,
        IUserSubscriptionRepository subscriptionRepository,
        IPromotionCodeRepository promotionCodeRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger)
    {
        _stripeService = stripeService;
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _promotionCodeRepository = promotionCodeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CreateCheckoutSessionResponse>> CreateCheckoutSessionAsync(
        Guid userId,
        string userEmail,
        Guid planDefinitionId,
        string? promoCode = null,
        CancellationToken cancellationToken = default)
    {
        var plan = await _subscriptionRepository.GetPlanDefinitionByIdAsync(planDefinitionId, cancellationToken);
        if (plan is null)
            return DomainError.NotFound($"Plano '{planDefinitionId}' não encontrado.");

        decimal? discountedPrice = null;
        decimal? discountAmount = null;

        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var promo = await _promotionCodeRepository.GetByCodeAsync(promoCode, cancellationToken);
            if (promo is null || !promo.IsValid())
                return DomainError.Invalid("Código promocional inválido ou expirado.");

            discountedPrice = promo.ApplyTo(plan.Price);
            discountAmount = plan.Price - discountedPrice;
        }

        var result = await _stripeService.CreateCheckoutSessionAsync(userId, userEmail, plan, discountedPrice, cancellationToken);

        _logger.LogInformation(
            "Sessão Stripe criada. SessionId: {SessionId}, UserId: {UserId}, Plano: {Plan}",
            result.SessionId, userId, plan.Name);

        return new CreateCheckoutSessionResponse(result.CheckoutUrl, plan.Price, discountedPrice ?? plan.Price, discountAmount);
    }

    public async Task<Result> HandleWebhookAsync(
        string payload,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        StripeWebhookEvent webhookEvent;

        try
        {
            webhookEvent = _stripeService.ParseWebhookEvent(payload, stripeSignature);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Assinatura de webhook Stripe inválida.");
            return DomainError.Unauthorized("Assinatura de webhook inválida.");
        }

        _logger.LogInformation("Webhook Stripe recebido. Tipo: {EventType}", webhookEvent.Type);

        switch (webhookEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutSessionCompletedAsync(webhookEvent, cancellationToken);
                break;

            case "checkout.session.async_payment_succeeded":
                await HandleAsyncPaymentSucceededAsync(webhookEvent, cancellationToken);
                break;

            case "checkout.session.async_payment_failed":
                await HandleAsyncPaymentFailedAsync(webhookEvent, cancellationToken);
                break;

            default:
                _logger.LogDebug("Evento Stripe ignorado: {EventType}", webhookEvent.Type);
                break;
        }

        return Result.Success();
    }

    private async Task HandleCheckoutSessionCompletedAsync(
        StripeWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var existing = await _paymentRepository.GetByStripeSessionIdAsync(webhookEvent.SessionId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("Webhook duplicado ignorado. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        var (userId, planDefinitionId) = ExtractMetadata(webhookEvent);

        var amountInBrl = webhookEvent.AmountTotal / 100m;
        var payment = Payment.Create(userId, planDefinitionId, webhookEvent.SessionId, amountInBrl, webhookEvent.PaymentIntentId);
        await _paymentRepository.AddAsync(payment, cancellationToken);

        if (webhookEvent.PaymentStatus != "paid")
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Boleto pendente. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        await ActivateSubscriptionAsync(payment, userId, planDefinitionId, webhookEvent, cancellationToken);
    }

    private async Task HandleAsyncPaymentSucceededAsync(
        StripeWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByStripeSessionIdAsync(webhookEvent.SessionId, cancellationToken);
        if (payment is null)
        {
            _logger.LogWarning("async_payment_succeeded sem pagamento pendente. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        if (payment.Status == Domain.Enums.PaymentStatus.Completed)
        {
            _logger.LogInformation("Pagamento já confirmado. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        var (userId, planDefinitionId) = ExtractMetadata(webhookEvent);
        await ActivateSubscriptionAsync(payment, userId, planDefinitionId, webhookEvent, cancellationToken);
    }

    private async Task HandleAsyncPaymentFailedAsync(
        StripeWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByStripeSessionIdAsync(webhookEvent.SessionId, cancellationToken);
        if (payment is null)
        {
            _logger.LogWarning("async_payment_failed sem pagamento pendente. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        payment.MarkAsFailed();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Pagamento falhou. SessionId: {SessionId}", webhookEvent.SessionId);
    }

    private async Task ActivateSubscriptionAsync(
        Payment payment,
        Guid userId,
        Guid planDefinitionId,
        StripeWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        var plan = await _subscriptionRepository.GetPlanDefinitionByIdAsync(planDefinitionId, cancellationToken)
            ?? throw new InvalidOperationException($"Plano '{planDefinitionId}' não encontrado no webhook.");

        payment.MarkAsCompleted(webhookEvent.PaymentIntentId ?? webhookEvent.SessionId);

        var paymentReference = webhookEvent.PaymentIntentId ?? webhookEvent.SessionId;
        var subscription = UserSubscription.Create(userId, planDefinitionId, paymentReference);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Plano ativado. UserId: {UserId}, Plano: {PlanName}, Valor: R$ {Amount}",
            userId, plan.Name, payment.Amount);
    }

    private static (Guid UserId, Guid PlanDefinitionId) ExtractMetadata(StripeWebhookEvent webhookEvent)
    {
        if (!webhookEvent.Metadata.TryGetValue("userId", out var userIdStr) ||
            !Guid.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("Metadata 'userId' ausente no evento do Stripe.");

        if (!webhookEvent.Metadata.TryGetValue("planDefinitionId", out var planIdStr) ||
            !Guid.TryParse(planIdStr, out var planDefinitionId))
            throw new InvalidOperationException("Metadata 'planDefinitionId' ausente no evento do Stripe.");

        return (userId, planDefinitionId);
    }
}
