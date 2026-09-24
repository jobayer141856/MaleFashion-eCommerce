using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.HeroSlides.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.HeroSlides.Queries
{
    [TestFixture]
    public class GetHeroSlideByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetHeroSlideByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetHeroSlideByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingSlide_ReturnsSlide()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var query = new GetHeroSlideByIdQuery { Id = slideId };
            var slide = new HeroSlide
            {
                Id = slideId,
                Title = "Banner Slide"
            };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetByIdAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slide)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(slideId),
                () => result!.Title.ShouldBe("Banner Slide"),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_SlideNotFound_ReturnsNull()
        {
            // Arrange
            var slideId = Guid.NewGuid();
            var query = new GetHeroSlideByIdQuery { Id = slideId };

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetByIdAsync(slideId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroSlide?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockHeroSlideRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

