using SaasStarter.Domain.Entities;

namespace SaasStarter.Application.Common.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByStripeSessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
