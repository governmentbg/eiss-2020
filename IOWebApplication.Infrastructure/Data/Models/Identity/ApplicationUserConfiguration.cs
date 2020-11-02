// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("identity_users")
                .HasKey(user => user.Id);

            // Indexes for "normalized" username and email, to allow efficient lookups
            builder.HasIndex(u => u.NormalizedUserName).HasName("user_name_index").IsUnique();
            builder.HasIndex(u => u.NormalizedEmail).HasName("email_index");

            // Each User can have many UserClaims
            builder.HasMany(e => e.Claims)
                .WithOne(e => e.User)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            // Each User can have many UserLogins
            builder.HasMany(e => e.Logins)
                .WithOne(e => e.User)
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();

            // Each User can have many UserTokens
            builder.HasMany(e => e.Tokens)
                .WithOne(e => e.User)
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();

            // Each User can have many entries in the UserRole join table
            builder.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            //builder.HasOne(u => u.PersonalIdType)
            //    .WithMany()
            //    .HasForeignKey(u => u.PersonalIdTypeId);

            builder.Property(p => p.Id)
                .HasColumnName("id");
            builder.Property(p => p.AccessFailedCount)
                .HasColumnName("access_failed_count");
            builder.Property(p => p.ConcurrencyStamp)
                .HasColumnName("concurrency_stamp")
                .IsConcurrencyToken();
            builder.Property(p => p.Email)
                .HasColumnName("email")
                .HasMaxLength(256);
            builder.Property(p => p.EmailConfirmed)
                .HasColumnName("email_confirmed");
            builder.Property(p => p.LockoutEnabled)
                .HasColumnName("lockout_enabled");
            builder.Property(p => p.LockoutEnd)
                .HasColumnName("lockout_end");
            builder.Property(p => p.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(256);
            builder.Property(p => p.NormalizedUserName)
                .HasColumnName("normalized_user_name")
                .HasMaxLength(256);
            builder.Property(p => p.PasswordHash)
                .HasColumnName("password_hash");
            builder.Property(p => p.PhoneNumber)
                .HasColumnName("phone_number");
            builder.Property(p => p.PhoneNumberConfirmed)
                .HasColumnName("phone_number_confirmed");
            builder.Property(p => p.SecurityStamp)
                .HasColumnName("security_stamp");
            builder.Property(p => p.TwoFactorEnabled)
                .HasColumnName("two_factor_enabled");
            builder.Property(p => p.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(256);
            //builder.Property(p => p.PersonalId)
            //    .HasColumnName("personal_id")
            //    .HasMaxLength(256);
            //builder.Property(p => p.PersonalIdTypeId)
            //    .HasColumnName("personal_id_type");
            builder.Property(p => p.MustChangePassword)
                .HasColumnName("must_change_password")
                .HasDefaultValue(false);
            builder.Property(p => p.LawUnitId)
              .HasColumnName("lawunit_id");
            builder.HasOne(u => u.LawUnit)
               .WithMany()
               .HasForeignKey(u => u.LawUnitId);

            builder.Property(p => p.CourtId)
              .HasColumnName("court_id");

            builder.HasOne(u => u.Court)
               .WithMany()
               .HasForeignKey(u => u.CourtId);

            builder.Property(p => p.UserSettings)
             .HasColumnName("user_settings");

            builder.Property(p => p.EissId)
             .HasColumnName("eiss_id");

            builder.Property(p => p.WorkNotificationToMail)
                    .HasColumnName("work_notification_to_mail")
                    .HasDefaultValue(false);

            builder.Property(p => p.IsActive)
                    .HasColumnName("is_active");
        }
    }
}
