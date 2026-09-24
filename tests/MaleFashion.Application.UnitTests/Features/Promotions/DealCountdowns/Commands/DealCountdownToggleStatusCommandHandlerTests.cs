using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Commands;
using MaleFashion.Domain.Entities.Promotions;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.DealCountdowns.Commands
{
    [TestFixture]
    public class DealCountdownToggleStatusCommandHandlerTests
    {
        private AutoMock _mock;
        private DealCountdownToggleStatusCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IDealCountdownRepository> _mockDealCountdownRepository;

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
            _mockDealCountdownRepository = _mock.Mock<IDealCountdownRepository>();
            _handler = _mock.Create<DealCountdownToggleStatusCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockDealCountdownRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ActiveDeal_TogglesToInactive()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var deal = new DealCountdown
            {
                Id = dealId,
                DealTitle = "Deal",
                IsActive = true
            };

            var command = new DealCountdownToggleStatusCommand { Id = dealId };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetByIdAsync(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deal)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.EditAsync(deal, It.IsAny<CancellationToken>()))
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
                () => deal.IsActive.ShouldBeFalse(),
                () => _mockDealCountdownRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DealNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var command = new DealCountdownToggleStatusCommand { Id = dealId };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetByIdAsync(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DealCountdown?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}
