using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.ValueOfObjects;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.DealCountdowns.Commands
{
    [TestFixture]
    public class DealCountdownUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private DealCountdownUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IDealCountdownRepository> _mockDealCountdownRepository;
        private Mock<IProductRepository> _mockProductRepository;

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
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _handler = _mock.Create<DealCountdownUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockDealCountdownRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingDeal_UpdatesSuccessfully()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var deal = new DealCountdown
            {
                Id = dealId,
                ProductId = productId,
                DealTitle = "Old Deal",
                SalePrice = new Money(50m, "BDT"),
                IsActive = true
            };

            var product = new Product
            {
                Id = productId,
                Name = "Deal Product",
                BasePrice = new Money(100m, "BDT")
            };

            var command = new DealCountdownUpdateCommand
            {
                Id = dealId,
                ProductId = productId,
                DealTitle = "Updated Deal",
                SalePrice = 75m,
                Currency = "BDT",
                IsActive = false
            };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetByIdAsync(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deal)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
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
                () => result.ShouldNotBeNull(),
                () => result.DealTitle.ShouldBe("Updated Deal"),
                () => result.SalePrice.Amount.ShouldBe(75m),
                () => result.IsActive.ShouldBeFalse(),
                () => _mockDealCountdownRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DealNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var command = new DealCountdownUpdateCommand
            {
                Id = dealId,
                ProductId = Guid.NewGuid(),
                DealTitle = "Deal",
                SalePrice = 50m
            };

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

        [Test]
        public async Task Handle_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dealId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var deal = new DealCountdown { Id = dealId, ProductId = productId };

            var command = new DealCountdownUpdateCommand
            {
                Id = dealId,
                ProductId = productId,
                DealTitle = "Deal",
                SalePrice = 50m
            };

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.GetByIdAsync(dealId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deal)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

