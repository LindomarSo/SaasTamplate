using Microsoft.EntityFrameworkCore;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;
using SaasStarter.Infra.Persistence;

namespace SaasStarter.Infra.Repositories;

public class PromotionCodeRepository : IPromotionCodeRepository
{
    private readonly AppDbContext _db;

    public PromotionCodeRepository(AppDbContext db) => _db = db;

    public async Task<PromotionCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _db.PromotionCodes.FirstOrDefaultAsync(
            x => x.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public async Task<PromotionCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.PromotionCodes.FindAsync([id], cancellationToken);

    public async Task AddAsync(PromotionCode promotionCode, CancellationToken cancellationToken = default)
        => await _db.PromotionCodes.AddAsync(promotionCode, cancellationToken);
}
