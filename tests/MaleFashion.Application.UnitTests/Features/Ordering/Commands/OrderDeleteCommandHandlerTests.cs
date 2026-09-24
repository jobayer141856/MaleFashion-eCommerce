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
    public class OrderDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private OrderDeleteCommandHandler _handler;
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
            _handler = _mock.Create<OrderDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CancelledOrder_DeletesOrderSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new OrderDeleteCommand { Id = orderId };
            var order = new Order { Id = orderId, Status = OrderStatus.Cancelled };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.Remove(order))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ActiveOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new OrderDeleteCommand { Id = orderId };
            var order = new Order { Id = orderId, Status = OrderStatus.Processing };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("cannot be deleted"),
                () => _mockOrderRepository.Verify(x => x.Remove(It.IsAny<Order>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

