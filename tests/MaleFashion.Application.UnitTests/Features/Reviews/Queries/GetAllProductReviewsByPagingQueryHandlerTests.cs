using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Features.Reviews.Queries;
using MaleFashion.Domain.Entities.Reviews;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Reviews.Queries
{
    [TestFixture]
    public class GetAllProductReviewsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllProductReviewsByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllProductReviewsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductReviewRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedProductReviews()
        {
            // Arrange
            var query = new GetAllProductReviewsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Great",
                StatusFilter = true
            };

            var reviews = new List<ProductReview>
            {
                new ProductReview { ReviewerName = "Charlie", Rating = 5 },
                new ProductReview { ReviewerName = "Dana", Rating = 4 }
            };

            (IList<ProductReview> Items, int TotalCount, int FilteredCount) expected = (reviews, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.ProductReviewRepository)
                .Returns(_mockProductReviewRepository.Object)
                .Verifiable();

            _mockProductReviewRepository.Setup(x => x.GetPagedReviews(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockProductReviewRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

