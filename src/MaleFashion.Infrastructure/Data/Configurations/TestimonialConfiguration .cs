using MaleFashion.Domain.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
    {
        public void Configure(EntityTypeBuilder<Testimonial> builder)
        {
            builder.ToTable("Testimonials");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.AuthorName).HasMaxLength(150).IsRequired();
            builder.Property(t => t.AuthorProfession).HasMaxLength(150);
            builder.Property(t => t.AvatarUrl).HasMaxLength(500);
            builder.Property(t => t.Quote).HasMaxLength(1000).IsRequired();
        }
    }
}
