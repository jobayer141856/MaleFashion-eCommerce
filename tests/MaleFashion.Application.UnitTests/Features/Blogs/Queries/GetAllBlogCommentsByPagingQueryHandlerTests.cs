using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Domain.Entities.Blogs;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Queries
{
    [TestFixture]
    public class GetAllBlogCommentsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllBlogCommentsByPagingQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IBlogCommentRepository> _mockBlogCommentRepository;

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
            _mockBlogCommentRepository = _mock.Mock<IBlogCommentRepository>();
            _handler = _mock.Create<GetAllBlogCommentsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogCommentRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedBlogComments()
        {
            // Arrange
            var query = new GetAllBlogCommentsByPagingQuery
            {
                Start = 0,
                Length = 10,
                SearchText = "good",
                StatusFilter = true
            };

            var comments = new List<BlogComment>
            {
                new BlogComment { Name = "User 1", CommentText = "Very good post" },
                new BlogComment { Name = "User 2", CommentText = "Good read" }
            };

            (IList<BlogComment> Items, int TotalCount, int FilteredCount) expected = (comments, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.BlogCommentRepository)
                .Returns(_mockBlogCommentRepository.Object)
                .Verifiable();

            _mockBlogCommentRepository.Setup(x => x.GetPagedComments(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockBlogCommentRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

