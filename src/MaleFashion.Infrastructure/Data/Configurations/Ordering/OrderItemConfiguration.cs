using MaleFashion.Domain.Entities.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Ordering
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ProductName).HasMaxLength(250).IsRequired();
            // Money Value Objects
            builder.OwnsOne(i => i.UnitPrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            builder.OwnsOne(i => i.TotalPrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("TotalPrice").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("TotalPriceCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            // Prevent catalog deletes from corrupting order history
            builder.HasOne(i => i.Product)
                   .WithMany()
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(i => i.ProductVariant)
                   .WithMany()
                   .HasForeignKey(i => i.ProductVariantId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

