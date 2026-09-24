using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.HeroSlides.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.HeroSlides.Queries
{
    [TestFixture]
    public class GetAllHeroSlidesByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllHeroSlidesByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllHeroSlidesByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockHeroSlideRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedHeroSlides()
        {
            // Arrange
            var query = new GetAllHeroSlidesByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Winter"
            };

            var slides = new List<HeroSlide>
            {
                new HeroSlide { Title = "Winter Collection" },
                new HeroSlide { Title = "Fall Collection" }
            };

            (IList<HeroSlide> Items, int TotalCount, int FilteredCount) expected = (slides, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.HeroSlideRepository)
                .Returns(_mockHeroSlideRepository.Object)
                .Verifiable();

            _mockHeroSlideRepository.Setup(x => x.GetPagedHeroSlides(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockHeroSlideRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

