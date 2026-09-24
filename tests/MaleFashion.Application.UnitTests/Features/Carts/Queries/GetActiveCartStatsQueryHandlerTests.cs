using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Queries
{
    [TestFixture]
    public class GetActiveCartStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetActiveCartStatsQueryHandler _handler;
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
            _handler = _mock.Create<GetActiveCartStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsActiveCartStats()
        {
            // Arrange
            var query = new GetActiveCartStatsQuery();
            var stats = new ActiveCartStatsDto
            {
                TotalActiveCarts = 10,
                IdleCount = 5,
                AbandonedCount = 2,
                RegisteredCartsCount = 8,
                GuestCartsCount = 9
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.TotalActiveCarts.ShouldBe(10),
                () => result.IdleCount.ShouldBe(5),
                () => result.AbandonedCount.ShouldBe(2),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
