using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using MaleFashion.Domain.Entities.Promotions;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.DealCountdowns.Queries
{
    [TestFixture]
    public class GetDealCountdownByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetDealCountdownByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetDealCountdownByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockDealCountdownRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingDeal_ReturnsDeal()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var query = new GetDealCountdownByIdQuery { Id = dealId };
            var deal = new DealCountdown
            {
                Id = dealId,
                DealTitle = "Mega Deal"
            };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetDealCountdownWithProductById(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deal)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(dealId),
                () => result!.DealTitle.ShouldBe("Mega Deal"),
                () => _mockDealCountdownRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_NonExistingDeal_ReturnsNull()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var query = new GetDealCountdownByIdQuery { Id = dealId };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetDealCountdownWithProductById(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DealCountdown?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockDealCountdownRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}
