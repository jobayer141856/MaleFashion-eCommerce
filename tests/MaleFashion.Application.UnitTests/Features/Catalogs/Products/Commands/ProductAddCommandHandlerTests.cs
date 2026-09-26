using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Products.Commands;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Domain.Entities.Catalogs;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Commands
{
    [TestFixture]
    public class ProductAddCommandHandlerTests
    {
        private AutoMock _mock;
        private ProductAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepository;

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
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _handler = _mock.Create<ProductAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UniqueSkuWithImagesAndVariants_AddsProductAndReturnsProduct()
        {
            // Arrange
            var command = new ProductAddCommand
            {
                Name = "Slim Fit Cotton Shirt",
                Sku = "SHIRT-SLIM-001",
                Description = "A comfortable cotton shirt",
                AdditionalInformation = "100% Organic Cotton",
                BasePrice = 49.99m,
                SalePrice = 39.99m,
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                IsActive = true,
                IsNewArrival = true,
                IsHotSale = false,
                PrimaryImageName = "primary.jpg",
                GalleryImageNames = new List<string> { "gallery1.jpg", "gallery2.jpg" },
                Variants = new List<ProductVariantDto>
                {
                    new ProductVariantDto
                    {
                        Size = "M",
                        ColorName = "Blue",
                        ColorHex = "#0000FF",
                        StockQuantity = 10,
                        LowStockQuantity = 2,
                        Price = 39.99m
                    }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.Name.ShouldBe(command.Name),
                () => result.Sku.ShouldBe(command.Sku),
                () => result.BasePrice.Amount.ShouldBe(command.BasePrice),
                () => result.SalePrice.ShouldNotBeNull(),
                () => result.SalePrice!.Amount.ShouldBe(command.SalePrice!.Value),
                () => result.Images.Count.ShouldBe(3),
                () => result.Variants.Count.ShouldBe(1),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateSku_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new ProductAddCommand
            {
                Name = "Duplicate SKU Product",
                Sku = "DUPLICATE-SKU",
                BasePrice = 10.0m
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("already exists"),
                () => _mockProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_VariantsWithOptionalPriceAndTags_SetsBasePriceFallbackAndPersistsTags()
        {
            // Arrange
            var command = new ProductAddCommand
            {
                Name = "Test Casual Pants",
                Sku = "PANTS-001",
                BasePrice = 100m,
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                Tags = new List<string> { "Clothing", "Pants" },
                Variants = new List<ProductVariantDto>
                {
                    new ProductVariantDto
                    {
                        Size = "M",
                        ColorName = "Black",
                        Price = null // No price specified -> should fallback to BasePrice 100m
                    },
                    new ProductVariantDto
                    {
                        Size = "L",
                        ColorName = "Navy",
                        Price = 120m // Custom price specified
                    }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object);

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Tags.Count.ShouldBe(2),
                () => result.Tags.Select(t => t.Name).ShouldContain("Clothing"),
                () => result.Tags.Select(t => t.Name).ShouldContain("Pants"),
                () => result.Variants.Count.ShouldBe(2),
                () => result.Variants.First(v => v.Size == "M").VariantPrice.ShouldNotBeNull(),
                () => result.Variants.First(v => v.Size == "M").VariantPrice!.Amount.ShouldBe(100m),
                () => result.Variants.First(v => v.Size == "L").VariantPrice.ShouldNotBeNull(),
                () => result.Variants.First(v => v.Size == "L").VariantPrice!.Amount.ShouldBe(120m),
                () => _mockProductRepository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once)
            );
        }
    }
}

