using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Ordering.Commands
{
    [TestFixture]
    public class CreateOrderCommandHandlerTests
    {
        private AutoMock _mock;
        private CreateOrderCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICartRepository> _mockCartRepository;
        private Mock<IOrderRepository> _mockOrderRepository;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private Mock<ICouponRepository> _mockCouponRepository;

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
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _mockOrderRepository = _mock.Mock<IOrderRepository>();
            _mockCustomerRepository = _mock.Mock<ICustomerRepository>();
            _mockCouponRepository = _mock.Mock<ICouponRepository>();
            _handler = _mock.Create<CreateOrderCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockCustomerRepository?.Reset();
            _mockOrderRepository?.Reset();
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_EmptyCart_ReturnsFailure()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                CustomerId = Guid.NewGuid(),
                SessionId = "sess-1"
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(command.CustomerId, command.SessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cart?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeFalse(),
                () => result.Message.ShouldBe("Your shopping cart is empty."),
                () => _mockCartRepository.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ValidOrder_DeductsStockClearsCartAndCreatesOrder()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var variant = new ProductVariant
            {
                Id = variantId,
                StockQuantity = 10,
                Size = "L",
                ColorName = "Black"
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        ProductVariantId = variantId,
                        ProductVariant = variant,
                        ProductName = "Oxford Shirt",
                        Quantity = 2,
                        UnitPrice = new Money(1000m, "BDT")
                    }
                }
            };

            var command = new CreateOrderCommand
            {
                CustomerId = customerId,
                SessionId = "sess-1",
                PaymentMethod = PaymentMethod.Cash,
                ShippingAddress = new Address("123 Road", null, "Dhaka", "Dhaka", "1200", "Bangladesh")
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, "sess-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.OrderId.ShouldNotBe(Guid.Empty),
                () => result.OrderNumber.ShouldNotBeNull(),
                () => variant.StockQuantity.ShouldBe(8),
                () => cart.Items.Count.ShouldBe(0),
                () => _mockCartRepository.VerifyAll(),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_InsufficientStock_ReturnsFailureMessage()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var variant = new ProductVariant
            {
                Id = variantId,
                StockQuantity = 1, // Only 1 available
                Size = "M",
                ColorName = "White"
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = Guid.NewGuid(),
                        ProductVariantId = variantId,
                        ProductVariant = variant,
                        ProductName = "T-Shirt",
                        Quantity = 3, // Requesting 3
                        UnitPrice = new Money(500m, "BDT")
                    }
                }
            };

            var command = new CreateOrderCommand
            {
                CustomerId = customerId,
                SessionId = "sess-1"
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, "sess-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeFalse(),
                () => result.Message.ShouldContain("Insufficient stock"),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}
