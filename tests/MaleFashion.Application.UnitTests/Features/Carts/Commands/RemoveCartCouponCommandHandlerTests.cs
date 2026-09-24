using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Domain.Entities.Carts;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Commands
{
    [TestFixture]
    public class RemoveCartCouponCommandHandlerTests
    {
        private AutoMock _mock;
        private RemoveCartCouponCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockCartRepository = _mock.Mock<ICartRepository>();
            _handler = _mock.Create<RemoveCartCouponCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CartWithCoupon_RemovesCouponAndReturnsTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new RemoveCartCouponCommand { CustomerId = customerId };
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AppliedCouponCode = "SAVE20"
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
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
                () => result.ShouldBeTrue(),
                () => cart.AppliedCouponCode.ShouldBeNull(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_NoCouponOnCart_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new RemoveCartCouponCommand { CustomerId = customerId };
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AppliedCouponCode = null
            };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetActiveCartByCustomerOrSessionAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeFalse(),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

