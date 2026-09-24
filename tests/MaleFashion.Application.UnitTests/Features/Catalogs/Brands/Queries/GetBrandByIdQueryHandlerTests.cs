using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Brands.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Brands.Queries
{
    [TestFixture]
    public class GetBrandByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetBrandByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetBrandByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_BrandExists_ReturnsBrand()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var query = new GetBrandByIdQuery { Id = brandId };
            var brand = new Brand
            {
                Id = brandId,
                Name = "Nike",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(brandId),
                () => result!.Name.ShouldBe("Nike"),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_BrandDoesNotExist_ReturnsNull()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var query = new GetBrandByIdQuery { Id = brandId };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Brand?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeNull(),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
