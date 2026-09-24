using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.NewsletterSubscribers.Commands
{
    [TestFixture]
    public class NewsletterSubscriberToggleStatusCommandHandlerTests
    {
        private AutoMock _mock;
        private NewsletterSubscriberToggleStatusCommandHandler _handler;
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
            _handler = _mock.Create<NewsletterSubscriberToggleStatusCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockNewsletterSubscriberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ActiveSubscriber_TogglesToInactive()
        {
            // Arrange
            var subscriberId = Guid.NewGuid();
            var subscriber = new NewsletterSubscriber
            {
                Id = subscriberId,
                Email = "user@example.com",
                IsActive = true
            };

            var command = new NewsletterSubscriberToggleStatusCommand { Id = subscriberId };

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetByIdAsync(subscriberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscriber)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.EditAsync(subscriber, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeFalse(),
                () => subscriber.IsActive.ShouldBeFalse(),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_SubscriberNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var subscriberId = Guid.NewGuid();
            var command = new NewsletterSubscriberToggleStatusCommand { Id = subscriberId };

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetByIdAsync(subscriberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((NewsletterSubscriber?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

