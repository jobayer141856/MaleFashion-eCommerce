using MaleFashion.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Customers
{
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("WishlistItems");
            builder.HasKey(w => w.Id);
            builder.HasOne(w => w.Product)
                   .WithMany()
                   .HasForeignKey(w => w.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(w => w.ProductVariant)
                   .WithMany()
                   .HasForeignKey(w => w.ProductVariantId)
                   .OnDelete(DeleteBehavior.Cascade);
            // Prevent duplicate wishlist entries for the exact same variant
            builder.HasIndex(w => new { w.CustomerId, w.ProductId, w.ProductVariantId })
                   .IsUnique();
        }
    }
}
