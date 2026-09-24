using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Products.Commands;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Ordering;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Commands
{
    [TestFixture]
    public class ProductDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private ProductDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IOrderRepository> _mockOrderRepository;
        private Mock<ICartRepository> _mockCartRepository;
        private Mock<IFileStorageService> _mockFileStorageService;
        private Mock<ILogger<ProductDeleteCommandHandler>> _mockLogger;

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
            _mockOrderRepository = _mock.Mock<IOrderRepository>();
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _mockLogger = _mock.Mock<ILogger<ProductDeleteCommandHandler>>();
            _handler = _mock.Create<ProductDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockCartRepository?.Reset();
            _mockOrderRepository?.Reset();
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ProductExistsWithoutConflicts_DeletesProductAndCleansImages()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductDeleteCommand { Id = productId };
            var product = new Product
            {
                Id = productId,
                Name = "Denim Jacket",
                Images = new List<ProductImage>
                {
                    new ProductImage { ImageUrl = "jacket1.jpg" },
                    new ProductImage { ImageUrl = "jacket2.jpg" }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Cart, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockProductRepository.Setup(x => x.RemoveAsync(productId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync(It.IsAny<string>(), "products", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockProductRepository.VerifyAll(),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.Verify(x => x.DeleteImageAsync("jacket1.jpg", "products", It.IsAny<CancellationToken>()), Times.Once),
                () => _mockFileStorageService.Verify(x => x.DeleteImageAsync("jacket2.jpg", "products", It.IsAny<CancellationToken>()), Times.Once)
            );
        }

        [Test]
        public async Task Handle_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductDeleteCommand { Id = productId };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Product with ID '{productId}' not found."),
                () => _mockProductRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_ProductInOrder_ThrowsDuplicateDataException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductDeleteCommand { Id = productId };
            var product = new Product { Id = productId, Name = "Shirt" };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("is part of 2 customer order(s)"),
                () => _mockProductRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_ProductInActiveCart_ThrowsDuplicateDataException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductDeleteCommand { Id = productId };
            var product = new Product { Id = productId, Name = "Shoes" };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Cart, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("currently in 3 active shopping cart(s)"),
                () => _mockProductRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_ImageDeletionFails_LogsWarningAndDoesNotThrow()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductDeleteCommand { Id = productId };
            var product = new Product
            {
                Id = productId,
                Name = "Belt",
                Images = new List<ProductImage> { new ProductImage { ImageUrl = "belt.jpg" } }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Cart, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockProductRepository.Setup(x => x.RemoveAsync(productId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync("belt.jpg", "products", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Simulated storage error"))
                .Verifiable();

            // Act - Should not throw
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockProductRepository.VerifyAll(),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.VerifyAll()
            );
        }
    }
}

