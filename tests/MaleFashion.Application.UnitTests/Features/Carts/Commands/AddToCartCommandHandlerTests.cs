using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Commands
{
    [TestFixture]
    public class AddToCartCommandHandlerTests
    {
        private AutoMock _mock;
        private AddToCartCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<ICartRepository> _mockCartRepository;

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
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _handler = _mock.Create<AddToCartCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidProduct_AddsNewItemToCartAndReturnsTrue()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var command = new AddToCartCommand
            {
                ProductId = productId,
                CustomerId = customerId,
                Quantity = 2
            };

            var product = new Product
            {
                Id = productId,
                Name = "Cotton T-Shirt",
                IsActive = true,
                BasePrice = new Money(25m, "BDT")
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem>()
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => cart.Items.Count.ShouldBe(1),
                () => cart.Items.First().Quantity.ShouldBe(2),
                () => _mockProductRepository.VerifyAll(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ExistingItemInCart_IncrementsQuantity()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var command = new AddToCartCommand
            {
                ProductId = productId,
                CustomerId = customerId,
                Quantity = 1
            };

            var product = new Product
            {
                Id = productId,
                Name = "Polo Shirt",
                IsActive = true,
                BasePrice = new Money(30m, "BDT")
            };

            var existingItem = new CartItem
            {
                ProductId = productId,
                Quantity = 2,
                UnitPrice = new Money(30m, "BDT")
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem> { existingItem }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => existingItem.Quantity.ShouldBe(3),
                () => _mockProductRepository.VerifyAll(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_InactiveOrNullProduct_ReturnsFalse()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new AddToCartCommand
            {
                ProductId = productId,
                Quantity = 1
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeFalse(),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}
