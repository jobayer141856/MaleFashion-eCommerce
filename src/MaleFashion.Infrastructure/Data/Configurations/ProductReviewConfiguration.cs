using MaleFashion.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews", t =>
            {
                // Rating constraint: 1 to 5 stars
                t.HasCheckConstraint("CK_ProductReview_Rating", "[Rating] >= 1 AND [Rating] <= 5");
            });
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Title).HasMaxLength(200);
            builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
            builder.Property(r => r.ReviewerName).HasMaxLength(150);
            builder.Property(r => r.ReviewerEmail).HasMaxLength(256);
            builder.HasOne(r => r.Product)
                   .WithMany(p => p.Reviews)
                   .HasForeignKey(r => r.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.Customer)
                   .WithMany(c => c.Reviews)
                   .HasForeignKey(r => r.CustomerId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
