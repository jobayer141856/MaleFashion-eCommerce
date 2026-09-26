using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Products.Commands;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Commands
{
    [TestFixture]
    public class ProductUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private ProductUpdateCommandHandler _handler;
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
            _handler = _mock.Create<ProductUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommandAndUniqueSku_UpdatesProductAndReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Name",
                Sku = "OLD-SKU",
                BasePrice = new Money(50m),
                CreatedAt = DateTime.UtcNow
            };

            var command = new ProductUpdateCommand
            {
                Id = productId,
                Name = "New Name",
                Sku = "NEW-SKU",
                Description = "Updated Desc",
                BasePrice = 60m,
                SalePrice = 55m,
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                IsActive = true,
                PrimaryImageName = "new-primary.jpg",
                Variants = new List<ProductVariantDto>()
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct)
                .Verifiable();

            _mockProductRepository.Setup(x => x.EditAsync(existingProduct, It.IsAny<CancellationToken>()))
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
                () => result.Name.ShouldBe(command.Name),
                () => result.Sku.ShouldBe(command.Sku),
                () => result.BasePrice.Amount.ShouldBe(command.BasePrice),
                () => result.SalePrice.ShouldNotBeNull(),
                () => result.SalePrice!.Amount.ShouldBe(command.SalePrice!.Value),
                () => result.UpdatedAt.ShouldNotBeNull(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateSku_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new ProductUpdateCommand
            {
                Id = Guid.NewGuid(),
                Sku = "EXISTING-SKU",
                Name = "Sample"
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("already exists"),
                () => _mockProductRepository.Verify(x => x.EditAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductUpdateCommand
            {
                Id = productId,
                Sku = "VALID-SKU",
                Name = "Sample"
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Product with ID '{productId}' not found."),
                () => _mockProductRepository.Verify(x => x.EditAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_UpdateVariantsWithPriceAndTags_UpdatesAggregateRootAndPersistsTags()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingVariantId = Guid.NewGuid();
            var existingVariant = new ProductVariant
            {
                Id = existingVariantId,
                ProductId = productId,
                Size = "M",
                ColorName = "Black",
                Sku = "TEST-PROD-M",
                StockQuantity = 10,
                lowStockQuantity = 2,
                VariantPrice = new Money(90m),
                CreatedAt = DateTime.UtcNow
            };

            var existingTag = new ProductTag
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Name = "OldTag",
                CreatedAt = DateTime.UtcNow
            };

            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product",
                Sku = "TEST-PROD",
                BasePrice = new Money(100m),
                CreatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant> { existingVariant },
                Tags = new List<ProductTag> { existingTag },
                Images = new List<ProductImage>()
            };

            var command = new ProductUpdateCommand
            {
                Id = productId,
                Name = "Updated Product",
                Sku = "TEST-PROD",
                BasePrice = 100m,
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                IsActive = true,
                Tags = new List<string> { "NewTag1", "NewTag2" },
                Variants = new List<ProductVariantDto>
                {
                    new ProductVariantDto
                    {
                        Id = existingVariantId,
                        Size = "M",
                        ColorName = "Black",
                        StockQuantity = 15,
                        Price = null // No price -> fallback to BasePrice 100m
                    },
                    new ProductVariantDto
                    {
                        Id = null,
                        Size = "L",
                        ColorName = "White",
                        StockQuantity = 20,
                        Price = 130m // Custom price 130m
                    }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object);

            _mockProductRepository.Setup(x => x.IsDuplicateSku(command.Sku, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockProductRepository.Setup(x => x.GetProductDetailsById(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _mockProductRepository.Setup(x => x.EditAsync(existingProduct, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Tags.Count.ShouldBe(2),
                () => result.Tags.Select(t => t.Name).ShouldContain("NewTag1"),
                () => result.Tags.Select(t => t.Name).ShouldContain("NewTag2"),
                () => result.Tags.Select(t => t.Name).ShouldNotContain("OldTag"),
                () => result.Variants.Count.ShouldBe(2),
                () => result.Variants.First(v => v.Id == existingVariantId).VariantPrice!.Amount.ShouldBe(100m),
                () => result.Variants.First(v => v.Size == "L").VariantPrice!.Amount.ShouldBe(130m),
                () => _mockProductRepository.Verify(x => x.EditAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once)
            );
        }
    }
}

