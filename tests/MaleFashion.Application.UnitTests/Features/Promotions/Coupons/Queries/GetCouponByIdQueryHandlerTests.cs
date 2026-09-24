using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using MaleFashion.Domain.Entities.Promotions;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.Coupons.Queries
{
    [TestFixture]
    public class GetCouponByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetCouponByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetCouponByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingCoupon_ReturnsCoupon()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var query = new GetCouponByIdQuery { Id = couponId };
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "TESTCODE"
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(couponId),
                () => result!.Code.ShouldBe("TESTCODE"),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_NonExistingCoupon_ReturnsNull()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var query = new GetCouponByIdQuery { Id = couponId };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockCouponRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}
