using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.NewsletterSubscribers.Queries
{
    [TestFixture]
    public class GetAllNewsletterSubscribersByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllNewsletterSubscribersByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllNewsletterSubscribersByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockNewsletterSubscriberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedSubscribers()
        {
            // Arrange
            var query = new GetAllNewsletterSubscribersByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "gmail"
            };

            var subscribers = new List<NewsletterSubscriber>
            {
                new NewsletterSubscriber { Email = "user1@gmail.com" },
                new NewsletterSubscriber { Email = "user2@gmail.com" }
            };

            (IList<NewsletterSubscriber> Items, int TotalCount, int FilteredCount) expected = (subscribers, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetPagedSubscribers(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

