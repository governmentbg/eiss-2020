using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
        {
            builder.ToTable("identity_role_claims")
                .HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");
            builder.Property(p => p.RoleId)
                .HasColumnName("role_id");
            builder.Property(p => p.ClaimType)
                .HasColumnName("claim_type");
            builder.Property(p => p.ClaimValue)
                .HasColumnName("claim_value");
        }
    }
}
