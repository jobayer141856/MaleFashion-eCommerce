using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Domain.Entities.Blogs;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Commands
{
    [TestFixture]
    public class BlogPostUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogPostUpdateCommandHandler _handler;
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
            _handler = _mock.Create<BlogPostUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPost_UpdatesSuccessfully()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new BlogPost
            {
                Id = postId,
                Title = "Old Title",
                Content = "Old Content",
                AuthorName = "Old Author",
                IsPublished = false,
                Tags = new List<BlogTag>
                {
                    new BlogTag { Id = Guid.NewGuid(), BlogPostId = postId, Name = "OldTag" }
                }
            };

            var command = new BlogPostUpdateCommand
            {
                Id = postId,
                Title = "New Title",
                Content = "New Content",
                AuthorName = "New Author",
                FeaturedImageUrl = "new_img.jpg",
                IsPublished = true,
                Tags = new List<string> { "UpdatedTag1", "UpdatedTag2" }
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.EditAsync(post, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Title.ShouldBe("New Title"),
                () => result.Content.ShouldBe("New Content"),
                () => result.AuthorName.ShouldBe("New Author"),
                () => result.FeaturedImageUrl.ShouldBe("new_img.jpg"),
                () => result.IsPublished.ShouldBeTrue(),
                () => result.Tags.Count.ShouldBe(2),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ExistingPost_PreservesExistingTagsAndRemovesMissingTags()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var existingTagId = Guid.NewGuid();
            var post = new BlogPost
            {
                Id = postId,
                Title = "Title",
                Content = "Content",
                AuthorName = "Author",
                Tags = new List<BlogTag>
                {
                    new BlogTag { Id = existingTagId, BlogPostId = postId, Name = "PreservedTag" },
                    new BlogTag { Id = Guid.NewGuid(), BlogPostId = postId, Name = "RemovedTag" }
                }
            };

            var command = new BlogPostUpdateCommand
            {
                Id = postId,
                Title = "Updated Title",
                Content = "Updated Content",
                AuthorName = "Author",
                Tags = new List<string> { "PreservedTag", "NewTag" }
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.EditAsync(post, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Tags.Count.ShouldBe(2),
                () => result.Tags.ShouldContain(t => t.Name == "PreservedTag" && t.Id == existingTagId),
                () => result.Tags.ShouldContain(t => t.Name == "NewTag"),
                () => result.Tags.ShouldNotContain(t => t.Name == "RemovedTag"),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ExistingPost_WithEmptyTags_RemovesAllTags()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = new BlogPost
            {
                Id = postId,
                Title = "Title",
                Content = "Content",
                AuthorName = "Author",
                Tags = new List<BlogTag>
                {
                    new BlogTag { Id = Guid.NewGuid(), BlogPostId = postId, Name = "Tag1" }
                }
            };

            var command = new BlogPostUpdateCommand
            {
                Id = postId,
                Title = "Title",
                Content = "Content",
                AuthorName = "Author",
                Tags = new List<string>()
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.EditAsync(post, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Tags.Count.ShouldBe(0),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_PostNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var command = new BlogPostUpdateCommand
            {
                Id = postId,
                Title = "New Title",
                Content = "New Content",
                AuthorName = "New Author"
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.GetBlogPostDetailsById(postId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlogPost?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

