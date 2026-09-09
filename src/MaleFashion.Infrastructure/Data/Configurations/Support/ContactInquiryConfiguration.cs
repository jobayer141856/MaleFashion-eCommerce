using MaleFashion.Domain.Entities.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Support
{
    public class ContactInquiryConfiguration : IEntityTypeConfiguration<ContactInquiry>
    {
        public void Configure(EntityTypeBuilder<ContactInquiry> builder)
        {
            builder.ToTable("ContactInquiries");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.Property(c => c.Message).HasMaxLength(3000).IsRequired();
        }
    }
}
