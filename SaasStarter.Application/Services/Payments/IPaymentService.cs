using SaasStarter.Application.Common;

namespace SaasStarter.Application.Services.Payments;

public interface IPaymentService
{
    Task<Result<CreateCheckoutSessionResponse>> CreateCheckoutSessionAsync(
        Guid userId,
        string userEmail,
        Guid planDefinitionId,
        string? promoCode = null,
        CancellationToken cancellationToken = default);

    Task<Result> HandleWebhookAsync(string payload, string stripeSignature, CancellationToken cancellationToken = default);
}
