using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Ordering.Commands;
using MaleFashion.Domain.Entities.Ordering;
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
    public class OrderUpdatePaymentResultCommandHandlerTests
    {
        private AutoMock _mock;
        private OrderUpdatePaymentResultCommandHandler _handler;
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
            _handler = _mock.Create<OrderUpdatePaymentResultCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_MatchingOrderNumber_UpdatesPaymentStatusAndTransactionId()
        {
            // Arrange
            var command = new OrderUpdatePaymentResultCommand
            {
                OrderNumber = "MF-20260925-ABCD",
                PaymentStatus = PaymentStatus.Paid,
                TransactionId = "TXN-99999",
                GatewayNotes = "Payment completed via SSLCommerz"
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "MF-20260925-ABCD",
                PaymentStatus = PaymentStatus.Pending
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Order> { order })
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
                () => order.TransactionId.ShouldBe("TXN-99999"),
                () => order.OrderNotes!.ShouldContain("SSLCommerz"),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_OrderNumberNotFound_ReturnsFalse()
        {
            // Arrange
            var command = new OrderUpdatePaymentResultCommand
            {
                OrderNumber = "NONEXISTENT",
                PaymentStatus = PaymentStatus.Failed
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Order>())
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
