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
    public class CouponAddCommandHandlerTests
    {
        private AutoMock _mock;
        private CouponAddCommandHandler _handler;
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
            _handler = _mock.Create<CouponAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsCouponSuccessfully()
        {
            // Arrange
            var command = new CouponAddCommand
            {
                Code = "SAVE20",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20m,
                MinimumSpend = 50m,
                UsageLimit = 100,
                StartDate = DateTime.UtcNow.AddDays(-1),
                ExpirationDate = DateTime.UtcNow.AddDays(10),
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.IsDuplicateCode("SAVE20", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.AddAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()))
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
                () => result.Code.ShouldBe("SAVE20"),
                () => result.DiscountType.ShouldBe(DiscountType.Percentage),
                () => result.DiscountValue.ShouldBe(20m),
                () => result.UsageLimit.ShouldBe(100),
                () => result.IsActive.ShouldBeTrue(),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateCode_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new CouponAddCommand
            {
                Code = "SAVE20",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20m
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.IsDuplicateCode("SAVE20", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var ex = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldContain("SAVE20");
        }

        [Test]
        public async Task Handle_EmptyCode_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new CouponAddCommand
            {
                Code = "   ",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 20m
            };

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldBe("Coupon code cannot be empty.");
        }

        [Test]
        public async Task Handle_InvalidPercentageDiscount_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new CouponAddCommand
            {
                Code = "INVALID",
                DiscountType = DiscountType.Percentage,
                DiscountValue = 120m
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.IsDuplicateCode("INVALID", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldContain("Percentage discount must be between 1% and 100%");
        }
    }
}

