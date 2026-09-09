using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Catalogs
{
    public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
    {
        public void Configure(EntityTypeBuilder<ProductTag> builder)
        {
            builder.ToTable("ProductTags");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        }
    }
}
