using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.NewsletterSubscribers.Commands
{
    [TestFixture]
    public class NewsletterSubscribeCommandHandlerTests
    {
        private AutoMock _mock;
        private NewsletterSubscribeCommandHandler _handler;
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
            _handler = _mock.Create<NewsletterSubscribeCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockNewsletterSubscriberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_NewSubscriber_SubscribesSuccessfully()
        {
            // Arrange
            var command = new NewsletterSubscribeCommand { Email = "user@example.com" };

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((NewsletterSubscriber?)null)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.AddAsync(It.IsAny<NewsletterSubscriber>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.Message.ShouldContain("Thank you for subscribing"),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_AlreadyActiveSubscriber_ReturnsSuccessWithAlreadySubscribedMessage()
        {
            // Arrange
            var command = new NewsletterSubscribeCommand { Email = "user@example.com" };
            var existing = new NewsletterSubscriber { Email = "user@example.com", IsActive = true };

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.Message.ShouldContain("already subscribed"),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_InactiveSubscriber_ReactivatesSubscription()
        {
            // Arrange
            var command = new NewsletterSubscribeCommand { Email = "user@example.com" };
            var existing = new NewsletterSubscriber { Email = "user@example.com", IsActive = false };

            _mockUnitOfWork.SetupGet(x => x.NewsletterSubscriberRepository)
                .Returns(_mockNewsletterSubscriberRepository.Object)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing)
                .Verifiable();

            _mockNewsletterSubscriberRepository.Setup(x => x.EditAsync(existing, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeTrue(),
                () => result.IsReactivated.ShouldBeTrue(),
                () => result.Message.ShouldContain("reactivated"),
                () => existing.IsActive.ShouldBeTrue(),
                () => _mockNewsletterSubscriberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var command = new NewsletterSubscribeCommand { Email = "   " };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Success.ShouldBeFalse(),
                () => result.Message.ShouldBe("A valid email address is required.")
            );
        }
    }
}

