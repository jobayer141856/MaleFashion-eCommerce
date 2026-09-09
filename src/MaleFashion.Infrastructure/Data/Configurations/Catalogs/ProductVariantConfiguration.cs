using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Catalogs
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");
            builder.HasKey(v => v.Id);
            builder.Property(v => v.Size).HasMaxLength(50).IsRequired();
            builder.Property(v => v.ColorName).HasMaxLength(100).IsRequired();
            builder.Property(v => v.ColorHex).HasMaxLength(20).IsRequired();
            builder.Property(v => v.Sku).HasMaxLength(100).IsRequired();
            // Mapping Money VariantPrice Value Object
            builder.OwnsOne(v => v.VariantPrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("VariantPrice").HasPrecision(18, 2);
                m.Property(x => x.Currency).HasColumnName("VariantPriceCurrency").HasMaxLength(10);
            });
        }
    }
}
