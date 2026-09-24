using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.NewsletterSubscribers.Queries
{
    [TestFixture]
    public class GetNewsletterSubscriberStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetNewsletterSubscriberStatsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<INewsletterSubscriberRepository> _mockNewsletterSubscriberRepository;

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
            _mockNewsletterSubscriberRepository = _mock.Mock<INewsletterSubscriberRepository>();
            _handler = _mock.Create<GetNewsletterSubscriberStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockNewsletterSubscriberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsSubscriberCounts()
        {
            // Arrange
            var query = new GetNewsletterSubscriberStatsQuery();
            (int Total, int Active, int Inactive) stats = (500, 450, 50);

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetSubscriberCountsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Total.ShouldBe(500),
                () => result.Active.ShouldBe(450),
                () => result.Inactive.ShouldBe(50),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

