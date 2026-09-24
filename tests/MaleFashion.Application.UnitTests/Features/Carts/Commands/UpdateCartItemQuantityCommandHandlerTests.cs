using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Features.Carts.Commands;
using MaleFashion.Domain.Entities.Carts;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Carts.Commands
{
    [TestFixture]
    public class UpdateCartItemQuantityCommandHandlerTests
    {
        private AutoMock _mock;
        private UpdateCartItemQuantityCommandHandler _handler;
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
            _handler = _mock.Create<UpdateCartItemQuantityCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCartRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidQuantities_UpdatesQuantitiesAndRemovesZeroQtyItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var item1Id = Guid.NewGuid();
            var item2Id = Guid.NewGuid();

            var command = new UpdateCartItemQuantityCommand
            {
                CustomerId = customerId,
                ItemQuantities = new Dictionary<Guid, int>
                {
                    { item1Id, 5 },
                    { item2Id, 0 } // Zero should be removed
                }
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Items = new List<CartItem>
                {
                    new CartItem { Id = item1Id, Quantity = 2 },
                    new CartItem { Id = item2Id, Quantity = 1 }
                }
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
                () => cart.Items.Count.ShouldBe(1),
                () => cart.Items.First().Id.ShouldBe(item1Id),
                () => cart.Items.First().Quantity.ShouldBe(5),
                () => _mockCartRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
