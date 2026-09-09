using MaleFashion.Domain.Entities.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            builder.Property(o => o.OrderNotes).HasMaxLength(1000);
            builder.Property(o => o.TransactionId).HasMaxLength(150);
            // Billing Address Value Object
            builder.OwnsOne(o => o.BillingAddress, a =>
            {
                a.Property(p => p.StreetAddress).HasColumnName("BillingStreetAddress").HasMaxLength(250).IsRequired();
                a.Property(p => p.ApartmentSuite).HasColumnName("BillingApartmentSuite").HasMaxLength(100);
                a.Property(p => p.City).HasColumnName("BillingCity").HasMaxLength(100).IsRequired();
                a.Property(p => p.State).HasColumnName("BillingState").HasMaxLength(100).IsRequired();
                a.Property(p => p.PostalCode).HasColumnName("BillingPostalCode").HasMaxLength(20).IsRequired();
                a.Property(p => p.Country).HasColumnName("BillingCountry").HasMaxLength(100).IsRequired();
            });
            // Shipping Address Value Object
            builder.OwnsOne(o => o.ShippingAddress, a =>
            {
                a.Property(p => p.StreetAddress).HasColumnName("ShippingStreetAddress").HasMaxLength(250).IsRequired();
                a.Property(p => p.ApartmentSuite).HasColumnName("ShippingApartmentSuite").HasMaxLength(100);
                a.Property(p => p.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
                a.Property(p => p.State).HasColumnName("ShippingState").HasMaxLength(100).IsRequired();
                a.Property(p => p.PostalCode).HasColumnName("ShippingPostalCode").HasMaxLength(20).IsRequired();
                a.Property(p => p.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
            });
            // Money Value Objects
            builder.OwnsOne(o => o.Subtotal, m =>
            {
                m.Property(x => x.Amount).HasColumnName("Subtotal").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("SubtotalCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            builder.OwnsOne(o => o.TotalAmount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2).IsRequired();
                m.Property(x => x.Currency).HasColumnName("TotalAmountCurrency").HasMaxLength(10).HasDefaultValue("BDT");
            });
            builder.OwnsOne(o => o.ShippingCost, m =>
            {
                m.Property(x => x.Amount).HasColumnName("ShippingCost").HasPrecision(18, 2);
                m.Property(x => x.Currency).HasColumnName("ShippingCostCurrency").HasMaxLength(10);
            });
            builder.OwnsOne(o => o.DiscountAmount, m =>
            {
                m.Property(x => x.Amount).HasColumnName("DiscountAmount").HasPrecision(18, 2);
                m.Property(x => x.Currency).HasColumnName("DiscountAmountCurrency").HasMaxLength(10);
            });
            // Relationships
            builder.HasOne(o => o.Customer)
                   .WithMany(c => c.Orders)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(o => o.Coupon)
                   .WithMany()
                   .HasForeignKey(o => o.CouponId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.HasMany(o => o.OrderItems)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(o => o.OrderNumber).IsUnique();
        }
    }
}
