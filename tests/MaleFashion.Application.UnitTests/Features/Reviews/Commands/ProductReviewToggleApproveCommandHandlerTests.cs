using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Reviews.Commands;
using MaleFashion.Domain.Entities.Reviews;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Reviews.Commands
{
    [TestFixture]
    public class ProductReviewToggleApproveCommandHandlerTests
    {
        private AutoMock _mock;
        private ProductReviewToggleApproveCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockProductReviewRepository = _mock.Mock<IProductReviewRepository>();
            _handler = _mock.Create<ProductReviewToggleApproveCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductReviewRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingReview_TogglesApprovalAndRecalculatesRating()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var review = new ProductReview
            {
                Id = reviewId,
                ProductId = productId,
                IsApproved = false
            };

            var command = new ProductReviewToggleApproveCommand { Id = reviewId };

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.RecalculateProductRatingAsync(productId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => review.IsApproved.ShouldBeTrue(),
                () => _mockProductReviewRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ReviewNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var command = new ProductReviewToggleApproveCommand { Id = reviewId };

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductReview?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

