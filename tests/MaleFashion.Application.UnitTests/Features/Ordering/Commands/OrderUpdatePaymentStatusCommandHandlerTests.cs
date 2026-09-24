using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Ordering.Commands
{
    [TestFixture]
    public class OrderUpdatePaymentStatusCommandHandlerTests
    {
        private AutoMock _mock;
        private OrderUpdatePaymentStatusCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IOrderRepository> _mockOrderRepository;

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
            _handler = _mock.Create<OrderUpdatePaymentStatusCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidOrder_UpdatesPaymentStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new OrderUpdatePaymentStatusCommand
            {
                Id = orderId,
                PaymentStatus = PaymentStatus.Paid,
                TransactionId = "TX-12345"
            };

            var order = new Order
            {
                Id = orderId,
                PaymentStatus = PaymentStatus.Pending
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => order.PaymentStatus.ShouldBe(PaymentStatus.Paid),
                () => order.TransactionId.ShouldBe("TX-12345"),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_OrderNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new OrderUpdatePaymentStatusCommand
            {
                Id = orderId,
                PaymentStatus = PaymentStatus.Paid
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
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

