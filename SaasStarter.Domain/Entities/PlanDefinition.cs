using SaasStarter.Domain.Common;

namespace SaasStarter.Domain.Entities;

public class PlanDefinition : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsRecommended { get; private set; }

    // TODO: adicione aqui os limites/features específicos do seu SaaS
    // Exemplo: public int MonthlyApiCalls { get; private set; }

    public ICollection<UserSubscription> Subscriptions { get; private set; } = [];

    private PlanDefinition() { }

    public static PlanDefinition Create(string name, string description, decimal price, bool isRecommended = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        return new PlanDefinition
        {
            Name = name,
            Description = description,
            Price = price,
            IsRecommended = isRecommended
        };
    }
}
