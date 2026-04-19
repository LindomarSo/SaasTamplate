namespace SaasStarter.Application.Services.Payments;

public record CreateCheckoutSessionRequest(Guid PlanId, string? PromoCode = null);
public record CreateCheckoutSessionResponse(string CheckoutUrl, decimal OriginalPrice, decimal FinalPrice, decimal? DiscountAmount);
