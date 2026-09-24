using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Domain.Entities.Blogs;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Commands
{
    [TestFixture]
    public class BlogPostTogglePublishCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogPostTogglePublishCommandHandler _handler;
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
            _handler = _mock.Create<BlogPostTogglePublishCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPost_TogglesPublishState()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new BlogPost
            {
                Id = postId,
                Title = "Blog Post",
                IsPublished = true
            };

            var command = new BlogPostTogglePublishCommand { Id = postId };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeFalse(),
                () => post.IsPublished.ShouldBeFalse(),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_PostNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var command = new BlogPostTogglePublishCommand { Id = postId };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlogPost?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

