using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Queries
{
    [TestFixture]
    public class GetCustomerCartQueryHandlerTests
    {
        private AutoMock _mock;
        private GetCustomerCartQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICartRepository> _mockCartRepository;
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
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _mockCouponRepository = _mock.Mock<ICouponRepository>();
            _handler = _mock.Create<GetCustomerCartQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCouponRepository?.Reset();
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CartWithDiscountCoupon_CalculatesCorrectDiscount()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerCartQuery { CustomerId = customerId };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AppliedCouponCode = "10PERCENT",
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "Formal Shirt",
                        Quantity = 2,
                        UnitPrice = new Money(100m, "BDT")
                    }
                }
            };

            var coupon = new Coupon
            {
                Code = "10PERCENT",
                IsActive = true,
                DiscountType = DiscountType.Percentage,
                DiscountValue = 10m,
                ExpirationDate = DateTime.UtcNow.AddDays(5)
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CouponRepository)
                .Returns(_mockCouponRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockCouponRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon> { coupon })
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Items.Count.ShouldBe(1),
                () => result.Subtotal.ShouldBe(200m),
                () => result.DiscountAmount.ShouldBe(20m),
                () => result.Total.ShouldBe(180m),
                () => _mockCartRepository.VerifyAll(),
                () => _mockCouponRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_EmptySessionAndCustomerId_ReturnsEmptyDto()
        {
            // Arrange
            var query = new GetCustomerCartQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Items.Count.ShouldBe(0),
                () => _mockUnitOfWork.Verify(x => x.CartRepository, Times.Never)
            );
        }
    }
}

