using MaleFashion.Domain.Entities.Blogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
    {
        public void Configure(EntityTypeBuilder<BlogPost> builder)
        {
            builder.ToTable("BlogPosts");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Title).HasMaxLength(300).IsRequired();
            builder.Property(b => b.AuthorName).HasMaxLength(150).IsRequired();
            builder.Property(b => b.FeaturedImageUrl).HasMaxLength(500);
            builder.Property(b => b.Content).IsRequired();
            builder.HasMany(b => b.Comments)
                   .WithOne(c => c.BlogPost)
                   .HasForeignKey(c => c.BlogPostId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(b => b.Tags)
                   .WithOne(t => t.BlogPost)
                   .HasForeignKey(t => t.BlogPostId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
