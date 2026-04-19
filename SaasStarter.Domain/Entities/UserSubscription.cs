using SaasStarter.Domain.Common;

namespace SaasStarter.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PlanDefinitionId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? PaymentReference { get; private set; }

    public PlanDefinition PlanDefinition { get; private set; } = null!;

    private UserSubscription() { }

    public static UserSubscription Create(
        Guid userId,
        Guid planDefinitionId,
        string? paymentReference = null,
        DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId inválido.", nameof(userId));
        if (planDefinitionId == Guid.Empty)
            throw new ArgumentException("PlanDefinitionId inválido.", nameof(planDefinitionId));

        return new UserSubscription
        {
            UserId = userId,
            PlanDefinitionId = planDefinitionId,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            PaymentReference = paymentReference
        };
    }

    public bool IsActive() => ExpiresAt is null || ExpiresAt > DateTime.UtcNow;
}
