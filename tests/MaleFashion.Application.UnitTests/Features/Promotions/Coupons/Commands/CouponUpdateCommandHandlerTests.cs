using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.Coupons.Commands;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.Coupons.Commands
{
    [TestFixture]
    public class CouponUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private CouponUpdateCommandHandler _handler;
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
            _handler = _mock.Create<CouponUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingCoupon_UpdatesSuccessfully()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "OLDCODE",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m,
                IsActive = true
            };

            var command = new CouponUpdateCommand
            {
                Id = couponId,
                Code = "NEWCODE",
                DiscountType = DiscountType.FixedAmount,
                DiscountValue = 50m,
                MinimumSpend = 200m,
                UsageLimit = 50,
                IsActive = false
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.IsDuplicateCode("NEWCODE", couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
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
                () => result.ShouldNotBeNull(),
                () => result.Code.ShouldBe("NEWCODE"),
                () => result.DiscountType.ShouldBe(DiscountType.FixedAmount),
                () => result.DiscountValue.ShouldBe(50m),
                () => result.IsActive.ShouldBeFalse(),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CouponNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var command = new CouponUpdateCommand
            {
                Id = couponId,
                Code = "NEWCODE",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m
            };

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

        [Test]
        public async Task Handle_DuplicateCode_ThrowsDuplicateDataException()
        {
            // Arrange
            var couponId = Guid.NewGuid();
            var coupon = new Coupon
            {
                Id = couponId,
                Code = "OLDCODE",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m
            };

            var command = new CouponUpdateCommand
            {
                Id = couponId,
                Code = "EXISTINGCODE",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 15m
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetByIdAsync(couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.IsDuplicateCode("EXISTINGCODE", couponId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var ex = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldContain("EXISTINGCODE");
        }
    }
}

