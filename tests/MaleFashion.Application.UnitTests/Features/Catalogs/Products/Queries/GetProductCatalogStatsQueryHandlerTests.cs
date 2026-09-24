using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Queries
{
    [TestFixture]
    public class GetProductCatalogStatsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetProductCatalogStatsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
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
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _handler = _mock.Create<GetProductCatalogStatsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsCatalogStats()
        {
            // Arrange
            var query = new GetProductCatalogStatsQuery();
            var stats = new ProductCatalogStatsDto
            {
                TotalProducts = 150,
                TotalStockUnits = 500,
                LowStockAlertCount = 5,
                OutOfStockCount = 2,
                ActiveDealsCount = 3
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCatalogStatsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stats)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.TotalProducts.ShouldBe(150),
                () => result.TotalStockUnits.ShouldBe(500),
                () => result.LowStockAlertCount.ShouldBe(5),
                () => result.OutOfStockCount.ShouldBe(2),
                () => result.ActiveDealsCount.ShouldBe(3),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
