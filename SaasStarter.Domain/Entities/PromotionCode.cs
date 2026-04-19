using SaasStarter.Domain.Common;
using SaasStarter.Domain.Enums;

namespace SaasStarter.Domain.Entities;

public class PromotionCode : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public DiscountType DiscountType { get; private set; }

    /// <summary>
    /// Para Percentage: 0–100. Para Fixed: valor em BRL.
    /// </summary>
    public decimal DiscountValue { get; private set; }

    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    private PromotionCode() { }

    public static PromotionCode Create(
        string code,
        DiscountType discountType,
        decimal discountValue,
        int? maxUses = null,
        DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(discountValue);

        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new ArgumentOutOfRangeException(nameof(discountValue), "Desconto percentual não pode exceder 100%.");

        return new PromotionCode
        {
            Code = code.Trim().ToUpperInvariant(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxUses = maxUses,
            UsedCount = 0,
            ExpiresAt = expiresAt,
            IsActive = true
        };
    }

    public bool IsValid() =>
        IsActive &&
        (MaxUses is null || UsedCount < MaxUses) &&
        (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    public decimal ApplyTo(decimal originalPrice)
    {
        var discounted = DiscountType == DiscountType.Percentage
            ? originalPrice * (1 - DiscountValue / 100m)
            : originalPrice - DiscountValue;

        return Math.Max(0, Math.Round(discounted, 2, MidpointRounding.AwayFromZero));
    }

    public void IncrementUsage() => UsedCount++;
    public void Deactivate() => IsActive = false;
}
