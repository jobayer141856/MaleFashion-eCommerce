using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Domain.Entities.Blogs;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Queries
{
    [TestFixture]
    public class GetBlogPostByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetBlogPostByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetBlogPostByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPost_ReturnsBlogPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var query = new GetBlogPostByIdQuery { Id = postId };
            var post = new BlogPost
            {
                Id = postId,
                Title = "Fashion Forward",
                AuthorName = "Alex"
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(postId),
                () => result!.Title.ShouldBe("Fashion Forward"),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_PostNotFound_ReturnsNull()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var query = new GetBlogPostByIdQuery { Id = postId };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlogPost?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockBlogPostRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

