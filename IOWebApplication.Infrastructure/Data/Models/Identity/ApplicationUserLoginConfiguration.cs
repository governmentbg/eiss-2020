using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
        {
            builder.ToTable("identity_user_logins")
                .HasKey(p => new { p.ProviderKey, p.LoginProvider });

            builder.Property(p => p.ProviderKey)
                .HasColumnName("provider_key")
                .HasMaxLength(128);
            builder.Property(p => p.UserId)
                .HasColumnName("user_id");
            builder.Property(p => p.LoginProvider)
                .HasColumnName("login_provider")
                .HasMaxLength(128);
            builder.Property(p => p.ProviderDisplayName)
                .HasColumnName("provider_display_name");
        }
    }
}
