using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Features.Reviews.Queries;
using MaleFashion.Domain.Entities.Reviews;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Reviews.Queries
{
    [TestFixture]
    public class GetProductReviewByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetProductReviewByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetProductReviewByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductReviewRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingReview_ReturnsReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var query = new GetProductReviewByIdQuery(reviewId);
            var review = new ProductReview
            {
                Id = reviewId,
                ReviewerName = "Eve",
                Rating = 5
            };

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetReviewWithDetailsById(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(review)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(reviewId),
                () => result!.ReviewerName.ShouldBe("Eve"),
                () => result!.Rating.ShouldBe(5),
                () => _mockProductReviewRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ReviewNotFound_ReturnsNull()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var query = new GetProductReviewByIdQuery(reviewId);

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetReviewWithDetailsById(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductReview?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockProductReviewRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

