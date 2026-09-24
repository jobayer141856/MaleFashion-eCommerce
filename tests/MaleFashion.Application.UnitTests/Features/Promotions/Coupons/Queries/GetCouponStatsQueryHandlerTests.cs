using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.Coupons.Queries
{
    [TestFixture]
    public class GetCouponStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetCouponStatsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockCouponRepository = _mock.Mock<ICouponRepository>();
            _handler = _mock.Create<GetCouponStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsCouponStats()
        {
            // Arrange
            var query = new GetCouponStatsQuery();
            (int Total, int Active, int Expired, int TotalUsed) stats = (50, 30, 10, 150);

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetCouponStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Total.ShouldBe(50),
                () => result.Active.ShouldBe(30),
                () => result.Expired.ShouldBe(10),
                () => result.TotalUsed.ShouldBe(150),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

