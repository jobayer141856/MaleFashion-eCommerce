using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Domain.Entities.Blogs;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Blogs.Commands
{
    [TestFixture]
    public class BlogPostAddCommandHandlerTests
    {
        private AutoMock _mock;
        private BlogPostAddCommandHandler _handler;
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
            _handler = _mock.Create<BlogPostAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBlogPostRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsBlogPostSuccessfully()
        {
            // Arrange
            var command = new BlogPostAddCommand
            {
                Title = "The Fashion Trends of 2026",
                Content = "Here are the top fashion trends...",
                AuthorName = "Jane Doe",
                FeaturedImageUrl = "trends2026.jpg",
                IsPublished = true,
                Tags = new List<string> { "Fashion", "Trends", "Style" }
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.AddAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()))
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
                () => result.Title.ShouldBe("The Fashion Trends of 2026"),
                () => result.AuthorName.ShouldBe("Jane Doe"),
                () => result.IsPublished.ShouldBeTrue(),
                () => result.Tags.Count.ShouldBe(3),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_EmptyAuthor_DefaultsToAdmin()
        {
            // Arrange
            var command = new BlogPostAddCommand
            {
                Title = "No Author Blog",
                Content = "Some content",
                AuthorName = "   ",
                Tags = new List<string>()
            };

            _mockUnitOfWork.SetupGet(x => x.BlogPostRepository)
                .Returns(_mockBlogPostRepository.Object)
                .Verifiable();

            _mockBlogPostRepository.Setup(x => x.AddAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()))
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
                () => result.AuthorName.ShouldBe("Admin"),
                () => _mockBlogPostRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

