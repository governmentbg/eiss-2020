using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            builder.ToTable("identity_user_roles")
                .HasKey(p => new { p.UserId, p.RoleId });

            builder.Property(p => p.RoleId)
                .HasColumnName("role_id");
            builder.Property(p => p.UserId)
                .HasColumnName("user_id");
        }
    }
}
