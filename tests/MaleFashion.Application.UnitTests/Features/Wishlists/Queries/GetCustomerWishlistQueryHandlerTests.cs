using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Features.Wishlists.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Wishlists.Queries
{
    [TestFixture]
    public class GetCustomerWishlistQueryHandlerTests
    {
        private AutoMock _mock;
        private GetCustomerWishlistQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockCustomerRepository = _mock.Mock<ICustomerRepository>();
            _handler = _mock.Create<GetCustomerWishlistQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CustomerHasWishlist_ReturnsWishlistItems()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var query = new GetCustomerWishlistQuery { CustomerId = customerId };

            var product = new Product
            {
                Id = productId,
                Name = "Classic Trench Coat",
                Sku = "COAT-001",
                IsActive = true,
                BasePrice = new Money(150m, "BDT"),
                Variants = new List<ProductVariant>()
            };

            var wishlistItem = new WishlistItem
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = productId,
                Product = product,
                CreatedAt = DateTime.UtcNow
            };

            var customer = new Customer
            {
                Id = customerId,
                Wishlist = new List<WishlistItem> { wishlistItem }
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerWithWishlistByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Items.Count.ShouldBe(1),
                () => result.Items[0].ProductName.ShouldBe("Classic Trench Coat"),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

