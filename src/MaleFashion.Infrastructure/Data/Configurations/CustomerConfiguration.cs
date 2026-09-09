using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.Id);
            // 1-to-1 relationship with ApplicationUser (nullable for guest checkout)
            builder.HasOne<ApplicationUser>()
                   .WithOne()
                   .HasForeignKey<Customer>(c => c.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.Property(c => c.PhoneNumber).HasMaxLength(30);
            builder.Property(c => c.Gender).HasMaxLength(20);
            // Relationships
            builder.HasMany(c => c.Addresses)
                   .WithOne(a => a.Customer)
                   .HasForeignKey(a => a.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Wishlist)
                   .WithOne(w => w.Customer)
                   .HasForeignKey(w => w.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Orders)
                   .WithOne(o => o.Customer)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.HasMany(c => c.Carts)
                   .WithOne(ct => ct.Customer)
                   .HasForeignKey(ct => ct.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Reviews)
                   .WithOne(r => r.Customer)
                   .HasForeignKey(r => r.CustomerId)
                   .OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(c => c.Email);
            builder.HasIndex(c => c.UserId);
        }
    }
}
