using SaasStarter.Application.Common.Interfaces;

namespace SaasStarter.Application.Services.Plans;

public class PlanService : IPlanService
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;

    public PlanService(IUserSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<IEnumerable<PublicPlanCatalogResponse>> GetPublicCatalogAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _subscriptionRepository.GetAllPlanDefinitionsAsync(cancellationToken);
        return plans.Select(p => new PublicPlanCatalogResponse(p.Id, p.Name, p.Description, p.Price, p.IsRecommended));
    }

    public async Task<IEnumerable<AvailablePlanResponse>> GetAvailablePlansAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var plans = await _subscriptionRepository.GetAllPlanDefinitionsAsync(cancellationToken);
        var currentSubscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        return plans.Select(p => new AvailablePlanResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.IsRecommended,
            currentSubscription?.PlanDefinitionId == p.Id));
    }

    public async Task<CurrentPlanResponse?> GetCurrentPlanAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (subscription is null) return null;

        return new CurrentPlanResponse(
            subscription.Id,
            subscription.PlanDefinition.Name,
            subscription.StartedAt,
            subscription.ExpiresAt);
    }
}
