using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using MaleFashion.Domain.Entities.Promotions;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.Coupons.Queries
{
    [TestFixture]
    public class GetAllCouponsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllCouponsByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllCouponsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedCoupons()
        {
            // Arrange
            var query = new GetAllCouponsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "SUMMER"
            };

            var coupons = new List<Coupon>
            {
                new Coupon { Code = "SUMMER10" },
                new Coupon { Code = "SUMMER20" }
            };

            (IList<Coupon> Items, int TotalCount, int FilteredCount) expectedResult = (coupons, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetPagedCoupons(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

