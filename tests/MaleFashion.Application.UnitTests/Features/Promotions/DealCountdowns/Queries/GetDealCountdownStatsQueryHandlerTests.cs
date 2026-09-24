using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.DealCountdowns.Queries
{
    [TestFixture]
    public class GetDealCountdownStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetDealCountdownStatsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IDealCountdownRepository> _mockDealCountdownRepository;

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
            _mockDealCountdownRepository = _mock.Mock<IDealCountdownRepository>();
            _handler = _mock.Create<GetDealCountdownStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockDealCountdownRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsDealCountdownStats()
        {
            // Arrange
            var query = new GetDealCountdownStatsQuery();
            (int Total, int Live, int Upcoming, int Expired) stats = (10, 4, 2, 4);

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetDealStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Total.ShouldBe(10),
                () => result.Live.ShouldBe(4),
                () => result.Upcoming.ShouldBe(2),
                () => result.Expired.ShouldBe(4),
                () => _mockDealCountdownRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

