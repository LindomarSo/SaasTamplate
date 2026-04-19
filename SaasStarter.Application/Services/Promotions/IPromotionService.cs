namespace SaasStarter.Application.Services.Promotions;

public interface IPromotionService
{
    Task<ValidatePromoCodeResponse> ValidateAsync(ValidatePromoCodeRequest request, CancellationToken cancellationToken = default);
    Task CreateAsync(CreatePromoCodeRequest request, CancellationToken cancellationToken = default);
}
