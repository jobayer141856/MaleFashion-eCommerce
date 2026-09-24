using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Queries
{
    [TestFixture]
    public class GetActiveDealsForProductsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetActiveDealsForProductsQueryHandler _handler;
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
            _handler = _mock.Create<GetActiveDealsForProductsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsDealsForProducts()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetActiveDealsForProductsQuery
            {
                ProductIds = new List<Guid> { productId }
            };

            var deals = new Dictionary<Guid, ProductActiveDealDto>
            {
                {
                    productId,
                    new ProductActiveDealDto
                    {
                        DealTitle = "Summer Sale",
                        DiscountPercent = 20m,
                        SalePrice = 80m
                    }
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetActiveDealsForProductIdsAsync(query.ProductIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deals)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result.ContainsKey(productId).ShouldBeTrue(),
                () => result[productId].DiscountPercent.ShouldBe(20m),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
