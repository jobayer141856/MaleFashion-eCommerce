using MaleFashion.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
            builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
            builder.Property(c => c.MinimumSpend).HasPrecision(18, 2);
            builder.HasIndex(c => c.Code).IsUnique();
        }
    }
}
