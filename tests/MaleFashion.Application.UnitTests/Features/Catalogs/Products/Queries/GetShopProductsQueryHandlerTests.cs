using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Products.Queries
{
    [TestFixture]
    public class GetShopProductsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetShopProductsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ISqlUtility> _mockSqlUtility;

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
            _mockSqlUtility = _mock.Mock<ISqlUtility>();
            _handler = _mock.Create<GetShopProductsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockSqlUtility?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsProductsWithTotalsFromStoredProcedure()
        {
            // Arrange
            var query = new GetShopProductsQuery
            {
                PageIndex = 1,
                PageSize = 12,
                SearchText = "Jacket",
                OrderBy = "PriceLowHigh"
            };

            var products = new List<ShopProductItemDto>
            {
                new ShopProductItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Winter Jacket",
                    BasePrice = 120m,
                    CategoryName = "Outerwear"
                }
            };

            var outValues = new Dictionary<string, object>
            {
                { "@Total", 1 },
                { "@TotalDisplay", 1 }
            };

            _mockUnitOfWork.SetupGet(x => x.SqlUtility)
                .Returns(_mockSqlUtility.Object)
                .Verifiable();

            _mockSqlUtility.Setup(x => x.QueryWithStoredProcedureAsync<ShopProductItemDto>(
                    "GetShopProducts",
                    It.IsAny<IDictionary<string, object?>>(),
                    It.IsAny<IDictionary<string, Type>>()))
                .ReturnsAsync((products, (IDictionary<string, object>)outValues))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Products.Count.ShouldBe(1),
                () => result.Total.ShouldBe(1),
                () => result.TotalDisplay.ShouldBe(1),
                () => result.Products[0].Name.ShouldBe("Winter Jacket"),
                () => _mockSqlUtility.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
