using MaleFashion.Domain.Entities.Blogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations.Blogs
{

    public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
    {
        public void Configure(EntityTypeBuilder<BlogComment> builder)
        {
            builder.ToTable("BlogComments");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.Property(c => c.Phone).HasMaxLength(30);
            builder.Property(c => c.CommentText).HasMaxLength(2000).IsRequired();
        }
    }
}
