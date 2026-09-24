using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Features.Reviews.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Reviews;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Reviews.Commands
{
    [TestFixture]
    public class ProductReviewAddCustomerCommandHandlerTests
    {
        private AutoMock _mock;
        private ProductReviewAddCustomerCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IProductReviewRepository> _mockProductReviewRepository;

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
            _mockProductReviewRepository = _mock.Mock<IProductReviewRepository>();
            _handler = _mock.Create<ProductReviewAddCustomerCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductReviewRepository?.Reset();
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ProductExists_AddsReviewSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Leather Jacket" };

            var command = new ProductReviewAddCustomerCommand
            {
                ProductId = productId,
                CustomerId = customerId,
                ReviewerName = "Alice",
                ReviewerEmail = "alice@example.com",
                Rating = 5,
                Title = "Superb Quality",
                Comment = "Loved the leather texture and stitching!"
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.AddAsync(It.IsAny<ProductReview>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.Message.ShouldContain("Thank you! Your review has been submitted"),
                () => _mockProductRepository.VerifyAll(),
                () => _mockProductReviewRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new ProductReviewAddCustomerCommand
            {
                ProductId = productId,
                ReviewerName = "Bob",
                ReviewerEmail = "bob@example.com",
                Rating = 4,
                Title = "Good",
                Comment = "Nice product"
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeFalse(),
                () => result.Message.ShouldBe("Product not found."),
                () => _mockProductRepository.VerifyAll()
            );
        }
    }
}

