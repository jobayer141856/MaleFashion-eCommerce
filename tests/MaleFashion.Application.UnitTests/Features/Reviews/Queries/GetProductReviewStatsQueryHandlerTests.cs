using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Features.Reviews.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Reviews.Queries
{
    [TestFixture]
    public class GetProductReviewStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetProductReviewStatsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductReviewRepository> _mockProductReviewRepository;

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
            _mockProductReviewRepository = _mock.Mock<IProductReviewRepository>();
            _handler = _mock.Create<GetProductReviewStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductReviewRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsReviewStats()
        {
            // Arrange
            var query = new GetProductReviewStatsQuery();
            (int Total, int Pending, int Approved, double AverageRating) stats = (100, 15, 85, 4.6);

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetReviewStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Total.ShouldBe(100),
                () => result.Pending.ShouldBe(15),
                () => result.Approved.ShouldBe(85),
                () => result.AverageRating.ShouldBe(4.6),
                () => _mockProductReviewRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

