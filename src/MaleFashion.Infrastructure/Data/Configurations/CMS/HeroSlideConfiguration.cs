using MaleFashion.Domain.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MaleFashion.Infrastructure.Data.Configurations.CMS
{
    public class HeroSlideConfiguration : IEntityTypeConfiguration<HeroSlide>
    {
        public void Configure(EntityTypeBuilder<HeroSlide> builder)
        {
            builder.ToTable("HeroSlides");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Subtitle)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(h => h.Title)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(h => h.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(h => h.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(h => h.ButtonText)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(h => h.TargetUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(h => h.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(h => h.IsActive)
                .HasDefaultValue(true);

            // Initial Seed Data
            builder.HasData(
                new HeroSlide
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Subtitle = "Summer Collection",
                    Title = "Fall - Winter Collections 2030",
                    Description = "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.",
                    ImageUrl = "malefashion/img/hero/hero-1.jpg",
                    ButtonText = "Shop now",
                    TargetUrl = "/Shop",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new HeroSlide
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Subtitle = "Summer Collection",
                    Title = "Fall - Winter Collections 2030",
                    Description = "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.",
                    ImageUrl = "malefashion/img/hero/hero-2.jpg",
                    ButtonText = "Shop now",
                    TargetUrl = "/Shop",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

