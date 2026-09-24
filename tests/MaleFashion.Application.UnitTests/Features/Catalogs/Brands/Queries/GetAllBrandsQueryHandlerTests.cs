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
    public class GetAllBrandsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllBrandsQueryHandler _handler;
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
            _handler = _mock.Create<GetAllBrandsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_NoFilters_ReturnsAllBrands()
        {
            // Arrange
            var query = new GetAllBrandsQuery();
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "Nike", IsActive = true },
                new Brand { Id = Guid.NewGuid(), Name = "Adidas", IsActive = false }
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(brands)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(2),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ActiveOnlyFilter_ReturnsOnlyActiveBrands()
        {
            // Arrange
            var query = new GetAllBrandsQuery { ActiveOnly = true };
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "Nike", IsActive = true },
                new Brand { Id = Guid.NewGuid(), Name = "Adidas", IsActive = false }
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(brands)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result[0].Name.ShouldBe("Nike"),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_SearchTextFilter_ReturnsMatchingBrands()
        {
            // Arrange
            var query = new GetAllBrandsQuery { SearchText = "Puma" };
            var brands = new List<Brand>
            {
                new Brand { Id = Guid.NewGuid(), Name = "Nike", IsActive = true },
                new Brand { Id = Guid.NewGuid(), Name = "Puma Sports", IsActive = true }
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(brands)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result[0].Name.ShouldBe("Puma Sports"),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

