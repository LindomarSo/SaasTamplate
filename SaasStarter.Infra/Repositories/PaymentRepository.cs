using Microsoft.EntityFrameworkCore;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;
using SaasStarter.Infra.Persistence;

namespace SaasStarter.Infra.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task<Payment?> GetByStripeSessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        => await _db.Payments.FirstOrDefaultAsync(x => x.StripeSessionId == sessionId, cancellationToken);

    public async Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
        => await _db.Payments.FirstOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntentId, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        => await _db.Payments.AddAsync(payment, cancellationToken);
}
