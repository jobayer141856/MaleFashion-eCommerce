using MaleFashion.Domain.Entities.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MaleFashion.Infrastructure.Data.Configurations.Support
{
    public class StoreLocationConfiguration : IEntityTypeConfiguration<StoreLocation>
    {
        public void Configure(EntityTypeBuilder<StoreLocation> builder)
        {
            builder.ToTable("StoreLocations");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Title).HasMaxLength(150).IsRequired();
            builder.Property(s => s.Address).HasMaxLength(500).IsRequired();
            builder.Property(s => s.PhoneNumber).HasMaxLength(50).IsRequired();
            builder.Property(s => s.Email).HasMaxLength(256);
            builder.Property(s => s.DisplayOrder).HasDefaultValue(0);
            builder.Property(s => s.IsActive).HasDefaultValue(true);
        }
    }
}

