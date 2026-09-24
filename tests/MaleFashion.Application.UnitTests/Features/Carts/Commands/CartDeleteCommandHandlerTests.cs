using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Exceptions;
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
    public class CartDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private CartDeleteCommandHandler _handler;
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
            _handler = _mock.Create<CartDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CartExists_RemovesCartAndReturnsTrue()
        {
            // Arrange
            var cartId = Guid.NewGuid();
            var command = new CartDeleteCommand { Id = cartId };
            var cart = new Cart { Id = cartId };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetByIdAsync(cartId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart)
                .Verifiable();

            _mockCartRepository.Setup(x => x.Remove(cart))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CartNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var cartId = Guid.NewGuid();
            var command = new CartDeleteCommand { Id = cartId };

            _mockUnitOfWork.SetupGet(x => x.CartRepository)
                .Returns(_mockCartRepository.Object)
                .Verifiable();

            _mockCartRepository.Setup(x => x.GetByIdAsync(cartId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cart?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Cart not found."),
                () => _mockCartRepository.Verify(x => x.Remove(It.IsAny<Cart>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

