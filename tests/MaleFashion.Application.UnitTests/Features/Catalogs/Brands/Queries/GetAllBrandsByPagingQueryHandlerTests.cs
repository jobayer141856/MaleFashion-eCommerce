using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Brands.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Brands.Queries
{
    [TestFixture]
    public class GetAllBrandsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllBrandsByPagingQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IBrandRepository> _mockBrandRepository;

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
            _mockBrandRepository = _mock.Mock<IBrandRepository>();
            _handler = _mock.Create<GetAllBrandsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedBrandsResult()
        {
            // Arrange
            var query = new GetAllBrandsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Test"
            };

            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "Test Brand 1", IsActive = true },
                new Brand { Id = Guid.NewGuid(), Name = "Test Brand 2", IsActive = true }
            };

            var expectedResult = ((IList<Brand>)brands, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetPagedBrands(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

