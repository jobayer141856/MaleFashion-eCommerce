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
    public class DealCountdownAddCommandHandlerTests
    {
        private AutoMock _mock;
        private DealCountdownAddCommandHandler _handler;
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
            _handler = _mock.Create<DealCountdownAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockDealCountdownRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsDealSuccessfully()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Deal Jacket",
                BasePrice = new Money(100m, "BDT")
            };

            var command = new DealCountdownAddCommand
            {
                ProductId = productId,
                DealTitle = "Winter Mega Deal",
                SalePrice = 60m,
                Currency = "BDT",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.DealCountdownRepository)
                .Returns(_mockDealCountdownRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            _mockDealCountdownRepository.Setup(x => x.AddAsync(It.IsAny<DealCountdown>(), It.IsAny<CancellationToken>()))
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
                () => result.ProductId.ShouldBe(productId),
                () => result.DealTitle.ShouldBe("Winter Mega Deal"),
                () => result.SalePrice.Amount.ShouldBe(60m),
                () => result.SalePrice.Currency.ShouldBe("BDT"),
                () => result.IsActive.ShouldBeTrue(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockDealCountdownRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DealCountdownAddCommand
            {
                ProductId = productId,
                DealTitle = "Invalid Deal",
                SalePrice = 50m
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_SalePriceGreaterThanOrEqualToBasePrice_ThrowsInvalidOperationException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Deal Jacket",
                BasePrice = new Money(100m, "BDT")
            };

            var command = new DealCountdownAddCommand
            {
                ProductId = productId,
                DealTitle = "Bad Deal",
                SalePrice = 120m
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product)
                .Verifiable();

            // Act & Assert
            var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            ex.Message.ShouldContain("must be lower than the product base price");
        }
    }
}

