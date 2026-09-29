using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Domain.Entities.Carts;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Queries
{
    [TestFixture]
    public class GetAllActiveCartsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllActiveCartsByPagingQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _handler = _mock.Create<GetAllActiveCartsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedActiveCarts()
        {
            // Arrange
            var query = new GetAllActiveCartsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                CartFilter = "active"
            };

            var carts = new List<Cart>
            {
                new Cart { Id = Guid.NewGuid() }
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetPagedActiveCarts(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IList<Cart>)carts, 1, 1))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(1),
                () => result.Item2.ShouldBe(1),
                () => result.Item3.ShouldBe(1),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_WithSortText_PassesSortTextToRepository()
        {
            // Arrange
            var query = new GetAllActiveCartsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SortText = "UpdatedAt desc",
                CartFilter = "all"
            };

            var carts = new List<Cart>
            {
                new Cart { Id = Guid.NewGuid(), UpdatedAt = DateTime.UtcNow }
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetPagedActiveCarts(It.Is<GetAllActiveCartsByPagingQuery>(q => q.SortText == "UpdatedAt desc"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IList<Cart>)carts, 1, 1))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(1),
                () => result.Item2.ShouldBe(1),
                () => result.Item3.ShouldBe(1),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

