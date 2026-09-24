using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.HeroSlides.Commands;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.HeroSlides.Commands
{
    [TestFixture]
    public class HeroSlideUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private HeroSlideUpdateCommandHandler _handler;
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
            _handler = _mock.Create<HeroSlideUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingSlide_UpdatesSuccessfully()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var slide = new HeroSlide
            {
                Id = slideId,
                Title = "Old Title",
                ButtonText = "Old Button"
            };

            var command = new HeroSlideUpdateCommand
            {
                Id = slideId,
                Title = "New Title",
                Subtitle = "New Subtitle",
                Description = "New Description",
                ImageUrl = "new.jpg",
                ButtonText = "Explore Now",
                TargetUrl = "/NewShop",
                DisplayOrder = 2,
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetByIdAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slide)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.EditAsync(slide, It.IsAny<CancellationToken>()))
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
                () => result.ButtonText.ShouldBe("Explore Now"),
                () => result.TargetUrl.ShouldBe("/NewShop"),
                () => result.ImageUrl.ShouldBe("new.jpg"),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_SlideNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var command = new HeroSlideUpdateCommand { Id = slideId, Title = "Title" };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetByIdAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroSlide?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

