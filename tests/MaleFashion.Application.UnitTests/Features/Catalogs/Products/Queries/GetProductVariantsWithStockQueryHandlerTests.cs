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
    public class GetProductVariantsWithStockQueryHandlerTests
    {
        private AutoMock _mock;
        private GetProductVariantsWithStockQueryHandler _handler;
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
            _handler = _mock.Create<GetProductVariantsWithStockQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsVariantsWithStock()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductVariantsWithStockQuery { ProductId = productId };
            var variants = new List<ProductVariantStockDto>
            {
                new ProductVariantStockDto
                {
                    Id = Guid.NewGuid(),
                    Size = "L",
                    ColorName = "Red",
                    StockQuantity = 15
                }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetVariantsByProductIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variants)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result[0].StockQuantity.ShouldBe(15),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
