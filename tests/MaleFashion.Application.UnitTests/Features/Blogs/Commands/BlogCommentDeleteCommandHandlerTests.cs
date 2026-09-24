using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Features.Blogs.Commands;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Commands
{
    [TestFixture]
    public class BlogCommentDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogCommentDeleteCommandHandler _handler;
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
            _handler = _mock.Create<BlogCommentDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogCommentRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_DeletesCommentAndReturnsTrue()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var command = new BlogCommentDeleteCommand { Id = commentId };

            _mockUnitOfWork.SetupGet(x => x.BlogCommentRepository)
                .Returns(_mockBlogCommentRepository.Object)
                .Verifiable();

            _mockBlogCommentRepository.Setup(x => x.RemoveAsync(commentId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => _mockBlogCommentRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

