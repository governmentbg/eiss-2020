using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
        {
            builder.ToTable("identity_user_claims")
                .HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");
            builder.Property(p => p.UserId)
                .HasColumnName("user_id");
            builder.Property(p => p.ClaimType)
                .HasColumnName("claim_type");
            builder.Property(p => p.ClaimValue)
                .HasColumnName("claim_value");
        }
    }
}
