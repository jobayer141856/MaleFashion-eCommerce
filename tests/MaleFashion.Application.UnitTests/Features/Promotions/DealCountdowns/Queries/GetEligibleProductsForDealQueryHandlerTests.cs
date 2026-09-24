using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Promotions.DealCountdowns.Queries
{
    [TestFixture]
    public class GetEligibleProductsForDealQueryHandlerTests
    {
        private AutoMock _mock;
        private GetEligibleProductsForDealQueryHandler _handler;
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
            _handler = _mock.Create<GetEligibleProductsForDealQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockProductRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsOnlyActiveProductsOrderedByName()
        {
            // Arrange
            var query = new GetEligibleProductsForDealQuery();
            var products = new List<Product>
            {
                new Product { Name = "Zebra Shirt", IsActive = true },
                new Product { Name = "Alpha Shoes", IsActive = false },
                new Product { Name = "Beta Pants", IsActive = true }
            };

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(products)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(2),
                () => result[0].Name.ShouldBe("Beta Pants"),
                () => result[1].Name.ShouldBe("Zebra Shirt"),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

