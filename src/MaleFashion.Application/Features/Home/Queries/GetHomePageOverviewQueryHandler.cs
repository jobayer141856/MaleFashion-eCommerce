using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Home.DTOs;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Home.Queries
{
    public class GetHomePageOverviewQueryHandler : IQueryHandler<GetHomePageOverviewQuery, HomePageOverviewDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetHomePageOverviewQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HomePageOverviewDto> Handle(GetHomePageOverviewQuery query, CancellationToken cancellationToken)
        {
            // 1. Dynamic Hero Slides
            List<HomeHeroSlideDto> heroSlideDtos;
            try
            {
                var heroSlides = await _unitOfWork.HeroSlideRepository.GetActiveSlidesAsync(cancellationToken);
                heroSlideDtos = (heroSlides != null && heroSlides.Count > 0)
                    ? heroSlides.Select(s => new HomeHeroSlideDto
                    {
                        Id = s.Id,
                        Subtitle = s.Subtitle,
                        Title = s.Title,
                        Description = s.Description,
                        ImageUrl = ResolveImageUrl(s.ImageUrl, "hero", "~/malefashion/img/hero/hero-1.jpg"),
                        ButtonText = s.ButtonText,
                        TargetUrl = s.TargetUrl
                    }).ToList()
                    : GetDefaultHeroSlides();
            }
            catch
            {
                heroSlideDtos = GetDefaultHeroSlides();
            }

            // 2. Dynamic Category Banners (Top 3 active categories)
            List<HomeCategoryBannerDto> bannerDtos;
            try
            {
                var allCategories = await _unitOfWork.CategoryRepository.GetAllAsync(cancellationToken);
                var activeCategories = allCategories?.Where(c => c.IsActive).ToList() ?? new List<Category>();
                bannerDtos = MapCategoryBanners(activeCategories);
            }
            catch
            {
                bannerDtos = GetDefaultBanners();
            }

            // 3. Dynamic Featured Products
            IList<Product>? products = null;
            List<HomeProductDto> productDtos;
            try
            {
                products = await _unitOfWork.ProductRepository.GetFeaturedHomeProductsAsync(8, cancellationToken);
                if (products != null && products.Count > 0)
                {
                    Dictionary<Guid, ProductActiveDealDto>? activeDeals = null;
                    try
                    {
                        var productIds = products.Select(p => p.Id).ToList();
                        activeDeals = await _unitOfWork.ProductRepository.GetActiveDealsForProductIdsAsync(productIds, cancellationToken);
                    }
                    catch
                    {
                        activeDeals = null;
                    }

                    productDtos = products.Select(p => MapProduct(p, activeDeals)).ToList();
                }
                else
                {
                    productDtos = GetDefaultProducts();
                }
            }
            catch
            {
                productDtos = GetDefaultProducts();
            }

            // 4. Dynamic Deal of the Week Countdown
            HomeDealCountdownDto dealDto;
            try
            {
                var dealEntity = await _unitOfWork.DealCountdownRepository.GetActiveDealOfTheWeekAsync(cancellationToken);
                dealDto = dealEntity != null
                    ? MapDealCountdown(dealEntity)
                    : GetDefaultDeal();
            }
            catch
            {
                dealDto = GetDefaultDeal();
            }

            // 5. Dynamic Latest News / Blogs
            List<HomeBlogDto> blogDtos;
            try
            {
                var (blogPosts, _) = await _unitOfWork.BlogPostRepository.GetPublishedBlogPosts(1, 3, null, null, cancellationToken);
                blogDtos = (blogPosts != null && blogPosts.Count > 0)
                    ? blogPosts.Select(MapBlog).ToList()
                    : GetDefaultBlogs();
            }
            catch
            {
                blogDtos = GetDefaultBlogs();
            }

            // 6. Dynamic Instagram Showcase (From recent product images or fallback)
            List<HomeInstagramDto> instagramDtos;
            try
            {
                instagramDtos = GetDynamicInstagramPosts(products);
            }
            catch
            {
                instagramDtos = GetDefaultInstagramPosts();
            }

            return new HomePageOverviewDto
            {
                HeroSlides = heroSlideDtos,
                Banners = bannerDtos,
                FeaturedProducts = productDtos,
                DealOfTheWeek = dealDto,
                LatestBlogs = blogDtos,
                InstagramPosts = instagramDtos
            };
        }

        #region Mapping Helpers

        private static List<HomeCategoryBannerDto> MapCategoryBanners(List<Category> categories)
        {
            var defaults = GetDefaultBanners();
            if (categories == null || categories.Count == 0)
                return defaults;

            var result = new List<HomeCategoryBannerDto>();
            string[] columnClasses = { "col-lg-7 offset-lg-4", "col-lg-5", "col-lg-7" };
            string[] cssModifiers = { "", "banner__item--middle", "banner__item--last" };
            string[] defaultImages = { "~/malefashion/img/banner/banner-1.jpg", "~/malefashion/img/banner/banner-2.jpg", "~/malefashion/img/banner/banner-3.jpg" };

            for (int i = 0; i < 3; i++)
            {
                if (i < categories.Count)
                {
                    var cat = categories[i];
                    string img = !string.IsNullOrWhiteSpace(cat.ImageName)
                        ? ResolveImageUrl(cat.ImageName, "categories", defaultImages[i])
                        : defaultImages[i];

                    result.Add(new HomeCategoryBannerDto
                    {
                        CategoryId = cat.Id,
                        Title = cat.Name,
                        ImageUrl = img,
                        TargetUrl = $"/Shop?categoryId={cat.Id}",
                        ColumnClass = columnClasses[i],
                        CssModifier = cssModifiers[i]
                    });
                }
                else
                {
                    result.Add(defaults[i]);
                }
            }

            return result;
        }

        private static HomeProductDto MapProduct(Product product, Dictionary<Guid, ProductActiveDealDto>? activeDeals = null)
        {
            var primaryImage = product.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? product.Images?.FirstOrDefault()?.ImageUrl;

            string resolvedImg = ResolveImageUrl(primaryImage, "products", "~/malefashion/img/product/product-1.jpg");

            var colors = product.Variants?
                .Where(v => !string.IsNullOrWhiteSpace(v.ColorHex))
                .Select(v => v.ColorHex.Trim())
                .Distinct()
                .Take(4)
                .ToList() ?? new List<string>();

            decimal basePriceAmount = product.BasePrice?.Amount ?? 0m;
            decimal? effectiveSalePrice = null;

            if (activeDeals != null && activeDeals.TryGetValue(product.Id, out var deal) && deal != null && deal.SalePrice > 0 && deal.SalePrice < basePriceAmount)
            {
                effectiveSalePrice = deal.SalePrice;
            }
            else if (product.SalePrice != null && product.SalePrice.Amount > 0 && product.SalePrice.Amount < basePriceAmount)
            {
                effectiveSalePrice = product.SalePrice.Amount;
            }

            return new HomeProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryName = product.Category?.Name,
                BasePrice = basePriceAmount,
                SalePrice = effectiveSalePrice,
                Currency = product.BasePrice?.Currency ?? "BDT",
                Rating = product.Rating,
                IsNewArrival = product.IsNewArrival,
                IsHotSale = product.IsHotSale,
                PrimaryImageUrl = resolvedImg,
                AvailableColors = colors
            };
        }

        private static HomeDealCountdownDto MapDealCountdown(DealCountdown deal)
        {
            var productImg = deal.Product?.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                ?? deal.Product?.Images?.FirstOrDefault()?.ImageUrl;

            string resolvedImg = ResolveImageUrl(productImg, "products", "~/malefashion/img/product-sale.png");

            return new HomeDealCountdownDto
            {
                DealId = deal.Id,
                ProductId = deal.ProductId,
                Title = !string.IsNullOrWhiteSpace(deal.DealTitle) ? deal.DealTitle : (deal.Product?.Name ?? "Multi-pocket Chest Bag Black"),
                SalePrice = deal.SalePrice?.Amount ?? 29.99m,
                OriginalPrice = deal.Product?.BasePrice?.Amount,
                Currency = deal.SalePrice?.Currency ?? "BDT",
                EndDate = deal.EndDate,
                ImageUrl = resolvedImg,
                TargetUrl = deal.ProductId != Guid.Empty ? $"/Shop/Details/{deal.ProductId}" : "/Shop"
            };
        }

        private static HomeBlogDto MapBlog(BlogPost post)
        {
            string resolvedImg = ResolveImageUrl(post.FeaturedImageUrl, "blogs", "~/malefashion/img/blog/blog-1.jpg");

            return new HomeBlogDto
            {
                Id = post.Id,
                Title = post.Title,
                PublishedDate = post.PublishedDate,
                ImageUrl = resolvedImg,
                TargetUrl = $"/BlogPosts/Details/{post.Id}"
            };
        }

        private static List<HomeInstagramDto> GetDynamicInstagramPosts(IList<Product>? products)
        {
            var result = new List<HomeInstagramDto>();
            if (products != null && products.Count > 0)
            {
                foreach (var product in products)
                {
                    if (result.Count >= 6) break;
                    var img = product.Images?.FirstOrDefault()?.ImageUrl;
                    if (!string.IsNullOrWhiteSpace(img))
                    {
                        result.Add(new HomeInstagramDto
                        {
                            ImageUrl = ResolveImageUrl(img, "products", "~/malefashion/img/instagram/instagram-1.jpg"),
                            TargetUrl = $"/Shop/Details/{product.Id}"
                        });
                    }
                }
            }

            // Fill remainder with defaults if needed
            string[] defaultImgs =
            {
                "~/malefashion/img/instagram/instagram-1.jpg",
                "~/malefashion/img/instagram/instagram-2.jpg",
                "~/malefashion/img/instagram/instagram-3.jpg",
                "~/malefashion/img/instagram/instagram-4.jpg",
                "~/malefashion/img/instagram/instagram-5.jpg",
                "~/malefashion/img/instagram/instagram-6.jpg"
            };

            for (int i = result.Count; i < 6; i++)
            {
                result.Add(new HomeInstagramDto
                {
                    ImageUrl = defaultImgs[i],
                    TargetUrl = "/Shop"
                });
            }

            return result;
        }

        private static List<HomeInstagramDto> GetDefaultInstagramPosts()
        {
            return new List<HomeInstagramDto>
            {
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-1.jpg", TargetUrl = "/Shop" },
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-2.jpg", TargetUrl = "/Shop" },
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-3.jpg", TargetUrl = "/Shop" },
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-4.jpg", TargetUrl = "/Shop" },
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-5.jpg", TargetUrl = "/Shop" },
                new() { ImageUrl = "~/malefashion/img/instagram/instagram-6.jpg", TargetUrl = "/Shop" }
            };
        }

        private static string ResolveImageUrl(string? rawPath, string uploadFolder, string fallbackPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                return fallbackPath;

            if (rawPath.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                rawPath.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                rawPath.StartsWith("~", StringComparison.OrdinalIgnoreCase))
            {
                return rawPath;
            }

            return $"/uploads/{uploadFolder}/{rawPath}";
        }

        #endregion

        #region Default Fallbacks

        private static List<HomeHeroSlideDto> GetDefaultHeroSlides()
        {
            return new List<HomeHeroSlideDto>
            {
                new()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Subtitle = "Summer Collection",
                    Title = "Fall - Winter Collections 2030",
                    Description = "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.",
                    ImageUrl = "~/malefashion/img/hero/hero-1.jpg",
                    ButtonText = "Shop now",
                    TargetUrl = "/Shop"
                },
                new()
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Subtitle = "Summer Collection",
                    Title = "Fall - Winter Collections 2030",
                    Description = "A specialist label creating luxury essentials. Ethically crafted with an unwavering commitment to exceptional quality.",
                    ImageUrl = "~/malefashion/img/hero/hero-2.jpg",
                    ButtonText = "Shop now",
                    TargetUrl = "/Shop"
                }
            };
        }

        private static List<HomeCategoryBannerDto> GetDefaultBanners()
        {
            return new List<HomeCategoryBannerDto>
            {
                new()
                {
                    Title = "Clothing Collections 2030",
                    ImageUrl = "~/malefashion/img/banner/banner-1.jpg",
                    TargetUrl = "/Shop",
                    ColumnClass = "col-lg-7 offset-lg-4",
                    CssModifier = ""
                },
                new()
                {
                    Title = "Accessories",
                    ImageUrl = "~/malefashion/img/banner/banner-2.jpg",
                    TargetUrl = "/Shop",
                    ColumnClass = "col-lg-5",
                    CssModifier = "banner__item--middle"
                },
                new()
                {
                    Title = "Shoes Spring 2030",
                    ImageUrl = "~/malefashion/img/banner/banner-3.jpg",
                    TargetUrl = "/Shop",
                    ColumnClass = "col-lg-7",
                    CssModifier = "banner__item--last"
                }
            };
        }

        private static List<HomeProductDto> GetDefaultProducts()
        {
            return new List<HomeProductDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Piqué Biker Jacket",
                    BasePrice = 67.24m,
                    Rating = 0,
                    IsNewArrival = true,
                    IsHotSale = false,
                    PrimaryImageUrl = "~/malefashion/img/product/product-1.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Piqué Biker Jacket",
                    BasePrice = 67.24m,
                    Rating = 0,
                    IsNewArrival = false,
                    IsHotSale = true,
                    PrimaryImageUrl = "~/malefashion/img/product/product-2.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Multi-pocket Chest Bag",
                    BasePrice = 55.00m,
                    SalePrice = 43.48m,
                    Rating = 4,
                    IsNewArrival = true,
                    IsHotSale = false,
                    PrimaryImageUrl = "~/malefashion/img/product/product-3.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Diagonal Textured Cap",
                    BasePrice = 60.90m,
                    Rating = 0,
                    IsNewArrival = false,
                    IsHotSale = true,
                    PrimaryImageUrl = "~/malefashion/img/product/product-4.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Lether Backpack",
                    BasePrice = 31.37m,
                    Rating = 0,
                    IsNewArrival = true,
                    IsHotSale = false,
                    PrimaryImageUrl = "~/malefashion/img/product/product-5.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Ankle Boots",
                    BasePrice = 120.00m,
                    SalePrice = 98.49m,
                    Rating = 4,
                    IsNewArrival = false,
                    IsHotSale = true,
                    PrimaryImageUrl = "~/malefashion/img/product/product-6.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "T-shirt Contrast Pocket",
                    BasePrice = 49.66m,
                    Rating = 0,
                    IsNewArrival = true,
                    IsHotSale = false,
                    PrimaryImageUrl = "~/malefashion/img/product/product-7.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Flowing Scarf",
                    BasePrice = 26.28m,
                    Rating = 0,
                    IsNewArrival = false,
                    IsHotSale = true,
                    PrimaryImageUrl = "~/malefashion/img/product/product-8.jpg",
                    AvailableColors = new() { "#e5ddc9", "#111111", "#d6d6d6" }
                }
            };
        }

        private static HomeDealCountdownDto GetDefaultDeal()
        {
            return new HomeDealCountdownDto
            {
                Title = "Multi-pocket Chest Bag Black",
                SalePrice = 29.99m,
                OriginalPrice = 49.99m,
                EndDate = DateTime.UtcNow.AddDays(3).AddHours(1).AddMinutes(50),
                ImageUrl = "~/malefashion/img/product-sale.png",
                TargetUrl = "/Shop"
            };
        }

        private static List<HomeBlogDto> GetDefaultBlogs()
        {
            return new List<HomeBlogDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "What Curling Irons Are The Best Ones",
                    PublishedDate = DateTime.Now.AddDays(-10),
                    ImageUrl = "~/malefashion/img/blog/blog-1.jpg",
                    TargetUrl = "/BlogPosts"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Eternity Bands Do Last Forever",
                    PublishedDate = DateTime.Now.AddDays(-8),
                    ImageUrl = "~/malefashion/img/blog/blog-2.jpg",
                    TargetUrl = "/BlogPosts"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "The Health Benefits Of Sunglasses",
                    PublishedDate = DateTime.Now.AddDays(-5),
                    ImageUrl = "~/malefashion/img/blog/blog-3.jpg",
                    TargetUrl = "/BlogPosts"
                }
            };
        }

        #endregion
    }
}
