using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Ordering.Commands
{
    [TestFixture]
    public class OrderCancelCommandHandlerTests
    {
        private AutoMock _mock;
        private OrderCancelCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IOrderRepository> _mockOrderRepository;
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
            _mockOrderRepository = _mock.Mock<IOrderRepository>();
            _mockCouponRepository = _mock.Mock<ICouponRepository>();
            _handler = _mock.Create<OrderCancelCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidOrder_CancelsOrderAndRestoresStock()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var couponId = Guid.NewGuid();
            var command = new OrderCancelCommand
            {
                Id = orderId,
                Reason = "Customer requested cancellation"
            };

            var variant = new ProductVariant { StockQuantity = 5 };
            var orderItem = new OrderItem
            {
                Quantity = 2,
                ProductVariant = variant
            };

            var order = new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                CouponId = couponId,
                OrderItems = new List<OrderItem> { orderItem }
            };

            var coupon = new Coupon { Id = couponId, UsedCount = 3 };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetOrderWithDetailsById(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => order.Status.ShouldBe(OrderStatus.Cancelled),
                () => order.OrderNotes!.ShouldContain("Customer requested cancellation"),
                () => variant.StockQuantity.ShouldBe(7),
                () => coupon.UsedCount.ShouldBe(2),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_OrderNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new OrderCancelCommand { Id = orderId };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetOrderWithDetailsById(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Order not found."),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}
