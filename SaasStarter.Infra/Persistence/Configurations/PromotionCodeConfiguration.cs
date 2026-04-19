using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Infra.Persistence.Configurations;

public class PromotionCodeConfiguration : IEntityTypeConfiguration<PromotionCode>
{
    public void Configure(EntityTypeBuilder<PromotionCode> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DiscountValue).HasColumnType("numeric(18,2)");

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
