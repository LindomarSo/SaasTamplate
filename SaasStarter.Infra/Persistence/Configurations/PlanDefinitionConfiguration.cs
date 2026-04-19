using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Infra.Persistence.Configurations;

public class PlanDefinitionConfiguration : IEntityTypeConfiguration<PlanDefinition>
{
    public void Configure(EntityTypeBuilder<PlanDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Price).HasColumnType("numeric(18,2)");

        builder.HasMany(x => x.Subscriptions)
               .WithOne(x => x.PlanDefinition)
               .HasForeignKey(x => x.PlanDefinitionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
