using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Services.Promotions;

public class PromotionService : IPromotionService
{
    private readonly IPromotionCodeRepository _promotionCodeRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PromotionService(
        IPromotionCodeRepository promotionCodeRepository,
        IUserSubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _promotionCodeRepository = promotionCodeRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ValidatePromoCodeResponse> ValidateAsync(ValidatePromoCodeRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _subscriptionRepository.GetPlanDefinitionByIdAsync(request.PlanId, cancellationToken);
        if (plan is null)
            return new ValidatePromoCodeResponse(false, null, null, "Plano não encontrado.");

        var promo = await _promotionCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (promo is null || !promo.IsValid())
            return new ValidatePromoCodeResponse(false, null, null, "Código inválido ou expirado.");

        var discountedPrice = promo.ApplyTo(plan.Price);
        var discountAmount = plan.Price - discountedPrice;

        return new ValidatePromoCodeResponse(true, discountedPrice, discountAmount, null);
    }

    public async Task CreateAsync(CreatePromoCodeRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _promotionCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Código '{request.Code}' já existe.");

        var promo = PromotionCode.Create(
            request.Code,
            request.DiscountType,
            request.DiscountValue,
            request.MaxUses,
            request.ExpiresAt);

        await _promotionCodeRepository.AddAsync(promo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
