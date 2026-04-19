using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasStarter.Domain.Entities;

namespace SaasStarter.Infra.Persistence.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentReference).HasMaxLength(200);

        builder.HasIndex(x => new { x.UserId, x.StartedAt });
    }
}
