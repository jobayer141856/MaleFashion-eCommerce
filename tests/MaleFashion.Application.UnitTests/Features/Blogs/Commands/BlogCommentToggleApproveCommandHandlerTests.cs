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
    public class BlogCommentToggleApproveCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogCommentToggleApproveCommandHandler _handler;
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
            _handler = _mock.Create<BlogCommentToggleApproveCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogCommentRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingComment_TogglesApprovalStatus()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var comment = new BlogComment
            {
                Id = commentId,
                IsApproved = false
            };

            var command = new BlogCommentToggleApproveCommand { Id = commentId };

            _mockUnitOfWork.SetupGet(x => x.BlogCommentRepository)
                .Returns(_mockBlogCommentRepository.Object)
                .Verifiable();

            _mockBlogCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comment)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => comment.IsApproved.ShouldBeTrue(),
                () => _mockBlogCommentRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CommentNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var command = new BlogCommentToggleApproveCommand { Id = commentId };

            _mockUnitOfWork.SetupGet(x => x.BlogCommentRepository)
                .Returns(_mockBlogCommentRepository.Object)
                .Verifiable();

            _mockBlogCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlogComment?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

