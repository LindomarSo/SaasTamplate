using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Common.Interfaces;

public interface IPromotionCodeRepository
{
    Task<PromotionCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<PromotionCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PromotionCode promotionCode, CancellationToken cancellationToken = default);
}
