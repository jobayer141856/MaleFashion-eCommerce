using MaleFashion.Domain.Entities.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
    {
        public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
        {
            builder.ToTable("NewsletterSubscribers");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(s => s.Email).IsUnique();
        }
    }
}
