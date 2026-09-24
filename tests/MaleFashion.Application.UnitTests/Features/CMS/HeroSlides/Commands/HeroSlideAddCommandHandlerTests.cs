using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.HeroSlides.Commands;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.HeroSlides.Commands
{
    [TestFixture]
    public class HeroSlideAddCommandHandlerTests
    {
        private AutoMock _mock;
        private HeroSlideAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IHeroSlideRepository> _mockHeroSlideRepository;

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
            _mockHeroSlideRepository = _mock.Mock<IHeroSlideRepository>();
            _handler = _mock.Create<HeroSlideAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsHeroSlideSuccessfully()
        {
            // Arrange
            var command = new HeroSlideAddCommand
            {
                Title = "Fall - Winter Collections 2026",
                Subtitle = "Summer Collection",
                Description = "A specialist label creating luxury essentials.",
                ImageUrl = "hero-1.jpg",
                ButtonText = "Shop now",
                TargetUrl = "/Shop",
                DisplayOrder = 1,
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.AddAsync(It.IsAny<HeroSlide>(), It.IsAny<CancellationToken>()))
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
                () => result.Title.ShouldBe("Fall - Winter Collections 2026"),
                () => result.ButtonText.ShouldBe("Shop now"),
                () => result.TargetUrl.ShouldBe("/Shop"),
                () => result.IsActive.ShouldBeTrue(),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_EmptyButtonAndUrl_UsesDefaults()
        {
            // Arrange
            var command = new HeroSlideAddCommand
            {
                Title = "Slide Title",
                ButtonText = "  ",
                TargetUrl = "  "
            };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.AddAsync(It.IsAny<HeroSlide>(), It.IsAny<CancellationToken>()))
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
                () => result.ButtonText.ShouldBe("Shop now"),
                () => result.TargetUrl.ShouldBe("/Shop"),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

