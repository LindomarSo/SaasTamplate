using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Common.Interfaces;

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlanDefinition>> GetAllPlanDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<PlanDefinition?> GetPlanDefinitionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(UserSubscription subscription, CancellationToken cancellationToken = default);
}
