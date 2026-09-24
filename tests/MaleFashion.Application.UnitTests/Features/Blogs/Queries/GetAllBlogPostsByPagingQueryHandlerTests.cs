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
    public class GetAllBlogPostsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllBlogPostsByPagingQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IBlogPostRepository> _mockBlogPostRepository;

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
            _mockBlogPostRepository = _mock.Mock<IBlogPostRepository>();
            _handler = _mock.Create<GetAllBlogPostsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedBlogPosts()
        {
            // Arrange
            var query = new GetAllBlogPostsByPagingQuery
            {
                Start = 0,
                Length = 10,
                SearchText = "Trends"
            };

            var posts = new List<BlogPost>
            {
                new BlogPost { Title = "Trends 2026" },
                new BlogPost { Title = "Summer Trends" }
            };

            (IList<BlogPost> Items, int TotalCount, int FilteredCount) expected = (posts, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetPagedBlogPosts(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

