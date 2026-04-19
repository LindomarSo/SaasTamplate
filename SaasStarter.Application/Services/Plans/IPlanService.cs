namespace SaasStarter.Application.Services.Plans;

public interface IPlanService
{
    Task<IEnumerable<PublicPlanCatalogResponse>> GetPublicCatalogAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailablePlanResponse>> GetAvailablePlansAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CurrentPlanResponse?> GetCurrentPlanAsync(Guid userId, CancellationToken cancellationToken = default);
}
