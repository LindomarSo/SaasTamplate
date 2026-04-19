namespace SaasStarter.Application.Services.Plans;

public record PublicPlanCatalogResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool IsRecommended);

public record AvailablePlanResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool IsRecommended,
    bool IsCurrentPlan);

public record CurrentPlanResponse(
    Guid SubscriptionId,
    string PlanName,
    DateTime StartedAt,
    DateTime? ExpiresAt);
