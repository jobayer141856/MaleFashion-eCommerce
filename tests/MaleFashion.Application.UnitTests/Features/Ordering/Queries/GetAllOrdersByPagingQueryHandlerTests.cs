using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Ordering;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Ordering.Queries
{
    [TestFixture]
    public class GetAllOrdersByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllOrdersByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllOrdersByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedOrders()
        {
            // Arrange
            var query = new GetAllOrdersByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "MF-2026"
            };

            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), OrderNumber = "MF-2026-001" }
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetPagedOrders(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IList<Order>)orders, 1, 1))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(1),
                () => result.Item2.ShouldBe(1),
                () => result.Item3.ShouldBe(1),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

