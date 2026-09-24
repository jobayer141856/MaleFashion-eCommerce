using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.Coupons.Commands;
using MaleFashion.Domain.Entities.Promotions;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.Coupons.Commands
{
    [TestFixture]
    public class CouponDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private CouponDeleteCommandHandler _handler;
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
            _handler = _mock.Create<CouponDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UnusedCoupon_DeletesSuccessfully()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "TESTCOUPON",
                UsedCount = 0
            };

            var command = new CouponDeleteCommand { Id = couponId };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.RemoveAsync(couponId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockCouponRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }

        [Test]
        public async Task Handle_RedeemedCoupon_ThrowsInvalidOperationException()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "REDEEMED",
                UsedCount = 3
            };

            var command = new CouponDeleteCommand { Id = couponId };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldContain("redeemed 3 time(s)");
        }

        [Test]
        public async Task Handle_CouponNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var command = new CouponDeleteCommand { Id = couponId };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}
