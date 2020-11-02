// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IOWebApplication.Infrastructure.Data.Models.Identity
{
    public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
        {
            builder.ToTable("identity_user_tokens")
                .HasKey(p => new { p.UserId, p.LoginProvider, p.Name });

            builder.Property(p => p.UserId)
                .HasColumnName("user_id");
            builder.Property(p => p.LoginProvider)
                .HasColumnName("login_provider")
                .HasMaxLength(128);
            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(128);
            builder.Property(p => p.Value)
                .HasColumnName("value");
        }
    }
}
