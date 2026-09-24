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
    public class CouponToggleStatusCommandHandlerTests
    {
        private AutoMock _mock;
        private CouponToggleStatusCommandHandler _handler;
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
            _handler = _mock.Create<CouponToggleStatusCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ActiveCoupon_TogglesToInactive()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "TOGGLE",
                IsActive = true
            };

            var command = new CouponToggleStatusCommand { Id = couponId };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.EditAsync(coupon, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeFalse(),
                () => coupon.IsActive.ShouldBeFalse(),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CouponNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var command = new CouponToggleStatusCommand { Id = couponId };

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
