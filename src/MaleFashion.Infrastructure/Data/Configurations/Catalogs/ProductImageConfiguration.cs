using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Catalogs
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.ImageUrl).HasMaxLength(500).IsRequired();
            builder.Property(i => i.VideoUrl).HasMaxLength(500);
        }
    }
}
