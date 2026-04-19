using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Infra.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StripeSessionId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");

        builder.HasIndex(x => x.StripeSessionId).IsUnique();
    }
}
