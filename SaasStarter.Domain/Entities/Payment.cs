using SaasStarter.Domain.Common;
using SaasStarter.Domain.Enums;

namespace SaasStarter.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid PlanDefinitionId { get; private set; }

    /// <summary>Stripe Checkout Session ID — usado para idempotência.</summary>
    public string StripeSessionId { get; private set; } = null!;

    /// <summary>Stripe PaymentIntent ID — disponível após pagamento aprovado.</summary>
    public string? StripePaymentIntentId { get; private set; }

    /// <summary>Valor cobrado em BRL.</summary>
    public decimal Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public PlanDefinition PlanDefinition { get; private set; } = null!;

    private Payment() { }

    public static Payment Create(
        Guid userId,
        Guid planDefinitionId,
        string stripeSessionId,
        decimal amount,
        string? stripePaymentIntentId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId inválido.", nameof(userId));
        if (planDefinitionId == Guid.Empty)
            throw new ArgumentException("PlanDefinitionId inválido.", nameof(planDefinitionId));
        if (string.IsNullOrWhiteSpace(stripeSessionId))
            throw new ArgumentException("StripeSessionId inválido.", nameof(stripeSessionId));

        return new Payment
        {
            UserId = userId,
            PlanDefinitionId = planDefinitionId,
            StripeSessionId = stripeSessionId,
            StripePaymentIntentId = stripePaymentIntentId,
            Amount = amount,
            Status = string.IsNullOrEmpty(stripePaymentIntentId)
                ? PaymentStatus.Pending
                : PaymentStatus.Completed
        };
    }

    public void MarkAsCompleted(string paymentIntentId)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("PaymentIntentId inválido.", nameof(paymentIntentId));

        StripePaymentIntentId = paymentIntentId;
        Status = PaymentStatus.Completed;
    }

    public void MarkAsFailed() => Status = PaymentStatus.Failed;
}
