using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Testimonials.Commands;
using MaleFashion.Domain.Entities.CMS;
using MapsterMapper;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Testimonials.Commands
{
    [TestFixture]
    public class TestimonialAddCommandHandlerTests
    {
        private AutoMock _mock;
        private TestimonialAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITestimonialRepository> _mockTestimonialRepository;
        private Mock<IMapper> _mockMapper;

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
            _mockTestimonialRepository = _mock.Mock<ITestimonialRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<TestimonialAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockTestimonialRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsTestimonialSuccessfully()
        {
            // Arrange
            var command = new TestimonialAddCommand
            {
                AuthorName = "Michael Brown",
                AuthorProfession = "Fashion Blogger",
                Quote = "Incredible quality and fast shipping!",
                AvatarUrl = "avatar.jpg"
            };

            var testimonial = new Testimonial
            {
                AuthorName = command.AuthorName,
                AuthorProfession = command.AuthorProfession,
                Quote = command.Quote,
                AvatarUrl = command.AvatarUrl
            };

            _mockMapper.Setup(x => x.Map<Testimonial>(command))
                .Returns(testimonial)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.AddAsync(testimonial, It.IsAny<CancellationToken>()))
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
                () => result.AuthorName.ShouldBe("Michael Brown"),
                () => result.AuthorProfession.ShouldBe("Fashion Blogger"),
                () => result.Quote.ShouldBe("Incredible quality and fast shipping!"),
                () => _mockMapper.VerifyAll(),
                () => _mockTestimonialRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CommandWithNullAvatar_UsesDefaultFallbackAvatar()
        {
            // Arrange
            var command = new TestimonialAddCommand
            {
                AuthorName = "Michael Brown",
                AuthorProfession = "Fashion Blogger",
                Quote = "Incredible quality and fast shipping!",
                AvatarUrl = null
            };

            var testimonial = new Testimonial
            {
                AuthorName = command.AuthorName,
                AuthorProfession = command.AuthorProfession,
                Quote = command.Quote,
                AvatarUrl = null!
            };

            _mockMapper.Setup(x => x.Map<Testimonial>(command))
                .Returns(testimonial)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.AddAsync(testimonial, It.IsAny<CancellationToken>()))
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
                () => result.AuthorName.ShouldBe("Michael Brown"),
                () => result.AuthorProfession.ShouldBe("Fashion Blogger"),
                () => result.Quote.ShouldBe("Incredible quality and fast shipping!"),
                () => result.AvatarUrl.ShouldBe("malefashion/img/about/testimonial-author.jpg"),
                () => _mockMapper.VerifyAll(),
                () => _mockTestimonialRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

