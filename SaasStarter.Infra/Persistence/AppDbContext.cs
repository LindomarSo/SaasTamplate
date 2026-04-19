using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Entities;
using SaasStarter.Domain.Identity;
using System.Reflection;

namespace SaasStarter.Infra.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlanDefinition> PlanDefinitions => Set<PlanDefinition>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PromotionCode> PromotionCodes => Set<PromotionCode>();

    // TODO: adicione aqui os DbSets das suas entidades de negócio
    // Exemplo: public DbSet<MyFeature> MyFeatures => Set<MyFeature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
