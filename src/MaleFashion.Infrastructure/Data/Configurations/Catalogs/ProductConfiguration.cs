using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Catalogs
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Description).IsRequired();
            // Mapping Money BasePrice Value Object
            builder.OwnsOne(p => p.BasePrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("BasePrice").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            // Mapping Money SalePrice Value Object (Nullable)
            builder.OwnsOne(p => p.SalePrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("SalePrice").HasPrecision(18, 2);
                m.Property(x => x.Currency).HasColumnName("SalePriceCurrency").HasMaxLength(10);
            });
            // Relationships
            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.Brand)
                   .WithMany(b => b.Products)
                   .HasForeignKey(p => p.BrandId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(p => p.Variants)
                   .WithOne(v => v.Product)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Product)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Tags)
                   .WithOne(t => t.Product)
                   .HasForeignKey(t => t.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.Reviews)
                   .WithOne(r => r.Product)
                   .HasForeignKey(r => r.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(p => p.Sku).IsUnique();
        }
    }
}
