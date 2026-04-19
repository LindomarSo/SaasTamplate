namespace SaasStarter.Application.Services.Payments;

public interface IPaymentService
{
    Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(
        Guid userId,
        string userEmail,
        Guid planDefinitionId,
        string? promoCode = null,
        CancellationToken cancellationToken = default);

    Task HandleWebhookAsync(string payload, string stripeSignature, CancellationToken cancellationToken = default);
}
