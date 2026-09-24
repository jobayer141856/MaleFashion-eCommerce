using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Ordering;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Ordering.Queries
{
    [TestFixture]
    public class GetOrderByOrderNumberQueryHandlerTests
    {
        private AutoMock _mock;
        private GetOrderByOrderNumberQueryHandler _handler;
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
            _handler = _mock.Create<GetOrderByOrderNumberQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidOrderNumber_ReturnsOrder()
        {
            // Arrange
            var query = new GetOrderByOrderNumberQuery { OrderNumber = "MF-2026-999" };
            var order = new Order { Id = Guid.NewGuid(), OrderNumber = "MF-2026-999" };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetOrderByOrderNumberWithDetailsAsync("MF-2026-999", It.IsAny<CancellationToken>()))
                .ReturnsAsync(order)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.OrderNumber.ShouldBe("MF-2026-999"),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_EmptyOrderNumber_ReturnsNull()
        {
            // Arrange
            var query = new GetOrderByOrderNumberQuery { OrderNumber = "" };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeNull(),
                () => _mockUnitOfWork.Verify(x => x.OrderRepository, Times.Never)
            );
        }
    }
}

