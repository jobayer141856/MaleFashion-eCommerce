using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
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
    public class BlogCommentAddCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogCommentAddCommandHandler _handler;
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
            _handler = _mock.Create<BlogCommentAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogCommentRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsCommentWithPendingApproval()
        {
            // Arrange
            var blogPostId = Guid.NewGuid();
            var command = new BlogCommentAddCommand
            {
                BlogPostId = blogPostId,
                Name = "John Doe",
                Email = "John.Doe@Example.com",
                Phone = "123456789",
                CommentText = "Great article!"
            };

            _mockUnitOfWork.SetupGet(x => x.BlogCommentRepository)
                .Returns(_mockBlogCommentRepository.Object)
                .Verifiable();

            _mockBlogCommentRepository.Setup(x => x.AddAsync(It.IsAny<BlogComment>(), It.IsAny<CancellationToken>()))
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
                () => result.BlogPostId.ShouldBe(blogPostId),
                () => result.Name.ShouldBe("John Doe"),
                () => result.Email.ShouldBe("john.doe@example.com"),
                () => result.CommentText.ShouldBe("Great article!"),
                () => result.IsApproved.ShouldBeFalse(),
                () => _mockBlogCommentRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

