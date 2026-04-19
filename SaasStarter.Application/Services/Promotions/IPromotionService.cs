using SaasStarter.Application.Common;

namespace SaasStarter.Application.Services.Promotions;

public interface IPromotionService
{
    Task<ValidatePromoCodeResponse> ValidateAsync(ValidatePromoCodeRequest request, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(CreatePromoCodeRequest request, CancellationToken cancellationToken = default);
}
