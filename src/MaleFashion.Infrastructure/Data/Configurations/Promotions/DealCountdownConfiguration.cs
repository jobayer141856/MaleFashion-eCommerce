using MaleFashion.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Promotions
{
    public class DealCountdownConfiguration : IEntityTypeConfiguration<DealCountdown>
    {
        public void Configure(EntityTypeBuilder<DealCountdown> builder)
        {
            builder.ToTable("DealCountdowns");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DealTitle).HasMaxLength(200).IsRequired();
            builder.OwnsOne(d => d.SalePrice, m =>
            {
                m.Property(x => x.Amount).HasColumnName("SalePrice").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("SalePriceCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            builder.HasOne(d => d.Product)
                   .WithMany()
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
