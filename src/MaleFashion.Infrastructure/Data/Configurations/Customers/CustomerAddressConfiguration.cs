using MaleFashion.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Customers
{
    public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ToTable("CustomerAddresses");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Type)
                   .HasConversion<string>()
                   .HasMaxLength(20);
            // Mapping Address Value Object as Owned Entity
            builder.OwnsOne(a => a.Address, addr =>
            {
                addr.Property(p => p.StreetAddress).HasColumnName("StreetAddress").HasMaxLength(250).IsRequired();
                addr.Property(p => p.ApartmentSuite).HasColumnName("ApartmentSuite").HasMaxLength(100);
                addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100).IsRequired();
                addr.Property(p => p.State).HasColumnName("State").HasMaxLength(100).IsRequired();
                addr.Property(p => p.PostalCode).HasColumnName("PostalCode").HasMaxLength(20).IsRequired();
                addr.Property(p => p.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
            });
        }
    }
}
