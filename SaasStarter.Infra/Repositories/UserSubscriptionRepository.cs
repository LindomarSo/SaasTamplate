using Microsoft.EntityFrameworkCore;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;
using SaasStarter.Infra.Persistence;

namespace SaasStarter.Infra.Repositories;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly AppDbContext _db;

    public UserSubscriptionRepository(AppDbContext db) => _db = db;

    public async Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _db.UserSubscriptions
            .Include(x => x.PlanDefinition)
            .Where(x => x.UserId == userId && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<UserSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.UserSubscriptions
            .Include(x => x.PlanDefinition)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IEnumerable<PlanDefinition>> GetAllPlanDefinitionsAsync(CancellationToken cancellationToken = default)
        => await _db.PlanDefinitions.OrderBy(x => x.Price).ToListAsync(cancellationToken);

    public async Task<PlanDefinition?> GetPlanDefinitionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.PlanDefinitions.FindAsync([id], cancellationToken);

    public async Task AddAsync(UserSubscription subscription, CancellationToken cancellationToken = default)
        => await _db.UserSubscriptions.AddAsync(subscription, cancellationToken);
}
