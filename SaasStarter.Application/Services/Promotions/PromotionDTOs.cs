using SaasStarter.Domain.Enums;

namespace SaasStarter.Application.Services.Promotions;

public record ValidatePromoCodeRequest(string Code, Guid PlanId);
public record ValidatePromoCodeResponse(bool IsValid, decimal? DiscountedPrice, decimal? DiscountAmount, string? ErrorMessage);

public record CreatePromoCodeRequest(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    DateTime? ExpiresAt);
