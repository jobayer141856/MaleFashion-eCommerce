using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Services;
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
    public class HeroSlideDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private HeroSlideDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IHeroSlideRepository> _mockHeroSlideRepository;
        private Mock<IFileStorageService> _mockFileStorageService;

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
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _handler = _mock.Create<HeroSlideDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingSlide_DeletesSlideAndImage()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var slide = new HeroSlide
            {
                Id = slideId,
                Title = "Hero Slide",
                ImageUrl = "slide1.jpg"
            };

            var command = new HeroSlideDeleteCommand { Id = slideId };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetByIdAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slide)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.RemoveAsync(slideId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync("slide1.jpg", "hero", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockHeroSlideRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
            _mockFileStorageService.VerifyAll();
        }

        [Test]
        public async Task Handle_SlideNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var command = new HeroSlideDeleteCommand { Id = slideId };

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

