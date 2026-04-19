using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Common.Interfaces;

public record StripeCheckoutSessionResult(string SessionId, string CheckoutUrl);

public record StripeWebhookEvent(
    string Type,
    string SessionId,
    string? PaymentIntentId,
    string? CustomerEmail,
    long AmountTotal,
    string PaymentStatus,
    Dictionary<string, string> Metadata);

public interface IStripeService
{
    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid userId,
        string userEmail,
        PlanDefinition plan,
        decimal? discountedPrice = null,
        CancellationToken cancellationToken = default);

    StripeWebhookEvent ParseWebhookEvent(string payload, string stripeSignature);
}
