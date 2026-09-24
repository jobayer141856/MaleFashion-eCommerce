using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Features.Wishlists.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Customers;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Wishlists.Commands
{
    [TestFixture]
    public class ToggleWishlistCommandHandlerTests
    {
        private AutoMock _mock;
        private ToggleWishlistCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<ICustomerRepository> _mockCustomerRepository;

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
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _mockCustomerRepository = _mock.Mock<ICustomerRepository>();
            _handler = _mock.Create<ToggleWishlistCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_AuthenticatedUserAddsNewItem_AddsToWishlistAndReturnsTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new ToggleWishlistCommand
            {
                CustomerId = customerId,
                ProductId = productId
            };

            var product = new Product { Id = productId, Name = "Sneakers", IsActive = true };
            var customer = new Customer
            {
                Id = customerId,
                Wishlist = new List<WishlistItem>()
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerWithWishlistByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.IsAdded.ShouldBeTrue(),
                () => customer.Wishlist.Count.ShouldBe(1),
                () => _mockProductRepository.VerifyAll(),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_AuthenticatedUserRemovesExistingItem_RemovesFromWishlist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new ToggleWishlistCommand
            {
                CustomerId = customerId,
                ProductId = productId
            };

            var product = new Product { Id = productId, Name = "Sneakers", IsActive = true };
            var existingItem = new WishlistItem
            {
                CustomerId = customerId,
                ProductId = productId
            };
            var customer = new Customer
            {
                Id = customerId,
                Wishlist = new List<WishlistItem> { existingItem }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetProductDetailsById(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerWithWishlistByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.IsAdded.ShouldBeFalse(),
                () => customer.Wishlist.Count.ShouldBe(0),
                () => _mockProductRepository.VerifyAll(),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

