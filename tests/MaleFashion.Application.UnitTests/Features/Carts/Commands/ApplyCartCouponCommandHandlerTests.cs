using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Commands
{
    [TestFixture]
    public class ApplyCartCouponCommandHandlerTests
    {
        private AutoMock _mock;
        private ApplyCartCouponCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICouponRepository> _mockCouponRepository;
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
            _mockCouponRepository = _mock.Mock<ICouponRepository>();
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _handler = _mock.Create<ApplyCartCouponCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockCouponRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCouponAndEligibleCart_AppliesCouponSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new ApplyCartCouponCommand
            {
                CustomerId = customerId,
                CouponCode = "SAVE10"
            };

            var coupon = new Coupon
            {
                Code = "SAVE10",
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                MinimumSpend = 100m
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        Quantity = 2,
                        UnitPrice = new Money(60m, "BDT")
                    }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon> { coupon })
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.Message.ShouldBe("Coupon applied successfully!"),
                () => cart.AppliedCouponCode.ShouldBe("SAVE10"),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ExpiredOrNonExistentCoupon_ReturnsFalse()
        {
            // Arrange
            var command = new ApplyCartCouponCommand
            {
                CouponCode = "INVALID_CODE"
            };

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon>())
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeFalse(),
                () => result.Message.ShouldBe("Invalid or expired coupon code."),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

