using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.HeroSlides.Commands;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.HeroSlides.Commands
{
    [TestFixture]
    public class HeroSlideToggleStatusCommandHandlerTests
    {
        private AutoMock _mock;
        private HeroSlideToggleStatusCommandHandler _handler;
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
            _handler = _mock.Create<HeroSlideToggleStatusCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_TogglesStatusAndReturnsNewState()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var command = new HeroSlideToggleStatusCommand { Id = slideId };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.ToggleStatusAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

