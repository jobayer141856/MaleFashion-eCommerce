using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Home.Queries;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Home.Queries
{
    [TestFixture]
    public class GetHomePageOverviewQueryHandlerTests
    {
        private AutoMock _mock;
        private GetHomePageOverviewQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IHeroSlideRepository> _mockHeroSlideRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IDealCountdownRepository> _mockDealCountdownRepository;
        private Mock<IBlogPostRepository> _mockBlogPostRepository;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mock = AutoMock.GetLoose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _mock?.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = _mock.Mock<IApplicationUnitOfWork>();
            _mockHeroSlideRepository = _mock.Mock<IHeroSlideRepository>();
            _mockCategoryRepository = _mock.Mock<ICategoryRepository>();
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _mockDealCountdownRepository = _mock.Mock<IDealCountdownRepository>();
            _mockBlogPostRepository = _mock.Mock<IBlogPostRepository>();
            _handler = _mock.Create<GetHomePageOverviewQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockDealCountdownRepository?.Reset();
            _mockProductRepository?.Reset();
            _mockCategoryRepository?.Reset();
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_WithRepositoryData_ReturnsPopulatedHomePageOverview()
        {
            // Arrange
            var query = new GetHomePageOverviewQuery();

            var heroSlides = new List<HeroSlide>
            {
                new HeroSlide
                {
                    Id = Guid.NewGuid(),
                    Title = "Summer 2026",
                    Subtitle = "Collection",
                    Description = "Cool fashion",
                    ImageUrl = "hero1.jpg",
                    ButtonText = "Shop",
                    TargetUrl = "/shop"
                }
            };

            var categories = new List<Category>
            {
                new Category { Name = "Clothing", IsActive = true },
                new Category { Name = "Accessories", IsActive = true },
                new Category { Name = "Shoes", IsActive = true }
            };

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "T-Shirt",
                    BasePrice = new Money(50m, "BDT"),
                    IsActive = true
                }
            };

            var dealProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Deal Bag",
                BasePrice = new Money(120m, "BDT")
            };

            var deal = new DealCountdown
            {
                Id = Guid.NewGuid(),
                DealTitle = "Deal of Week",
                SalePrice = new Money(70m, "BDT"),
                Product = dealProduct,
                EndDate = DateTime.UtcNow.AddDays(7),
                IsActive = true
            };

            var blogPosts = new List<BlogPost>
            {
                new BlogPost
                {
                    Id = Guid.NewGuid(),
                    Title = "Fashion Blog",
                    Content = "Sample blog content",
                    PublishedDate = DateTime.UtcNow,
                    IsPublished = true
                }
            };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository).Returns(_mockHeroSlideRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.CategoryRepository).Returns(_mockCategoryRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.ProductRepository).Returns(_mockProductRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository).Returns(_mockDealCountdownRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository).Returns(_mockBlogPostRepository.Object);

            _mockHeroSlideRepository.Setup(x => x.GetActiveSlidesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(heroSlides);

            _mockCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _mockProductRepository.Setup(x => x.GetFeaturedHomeProductsAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            _mockDealCountdownRepository.Setup(x => x.GetActiveDealOfTheWeekAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(deal);

            _mockBlogPostRepository.Setup(x => x.GetPublishedBlogPosts(1, 3, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((blogPosts, 1));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.HeroSlides.Count.ShouldBe(1),
                () => result.HeroSlides[0].Title.ShouldBe("Summer 2026"),
                () => result.Banners.Count.ShouldBeGreaterThan(0),
                () => result.FeaturedProducts.Count.ShouldBe(1),
                () => result.DealOfTheWeek.ShouldNotBeNull(),
                () => result.DealOfTheWeek.Title.ShouldBe("Deal of Week"),
                () => result.LatestBlogs.Count.ShouldBe(1)
            );
        }

        [Test]
        public async Task Handle_WhenRepositoriesReturnEmptyOrThrow_ReturnsFallbackDefaults()
        {
            // Arrange
            var query = new GetHomePageOverviewQuery();

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository).Returns(_mockHeroSlideRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.CategoryRepository).Returns(_mockCategoryRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.ProductRepository).Returns(_mockProductRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository).Returns(_mockDealCountdownRepository.Object);
            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository).Returns(_mockBlogPostRepository.Object);

            _mockHeroSlideRepository.Setup(x => x.GetActiveSlidesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable"));

            _mockCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable"));

            _mockProductRepository.Setup(x => x.GetFeaturedHomeProductsAsync(8, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable"));

            _mockDealCountdownRepository.Setup(x => x.GetActiveDealOfTheWeekAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable"));

            _mockBlogPostRepository.Setup(x => x.GetPublishedBlogPosts(1, 3, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB unavailable"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.HeroSlides.ShouldNotBeEmpty(),
                () => result.Banners.ShouldNotBeEmpty(),
                () => result.FeaturedProducts.ShouldNotBeEmpty(),
                () => result.DealOfTheWeek.ShouldNotBeNull(),
                () => result.LatestBlogs.ShouldNotBeEmpty(),
                () => result.InstagramPosts.ShouldNotBeEmpty()
            );
        }
    }
}
