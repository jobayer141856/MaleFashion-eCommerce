using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Brands.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Brands.Commands
{
    [TestFixture]
    public class BrandDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private BrandDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IBrandRepository> _mockBrandRepository;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IFileStorageService> _mockFileStorageService;
        private Mock<ILogger<BrandDeleteCommandHandler>> _mockLogger;

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
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _mockLogger = _mock.Mock<ILogger<BrandDeleteCommandHandler>>();
            _handler = _mock.Create<BrandDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockProductRepository?.Reset();
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_BrandExistsWithoutProductsAndWithLogo_DeletesBrandAndLogoSuccessfully()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandDeleteCommand { Id = brandId };
            var brand = new Brand
            {
                Id = brandId,
                Name = "Gucci",
                LogoUrl = "gucci-logo.png"
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.RemoveAsync(brandId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync(brand.LogoUrl, "brands", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockBrandRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_BrandExistsWithoutProductsAndNoLogo_DeletesBrandWithoutCallingFileStorage()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandDeleteCommand { Id = brandId };
            var brand = new Brand
            {
                Id = brandId,
                Name = "Zara",
                LogoUrl = null
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.RemoveAsync(brandId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockBrandRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_BrandNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandDeleteCommand { Id = brandId };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Brand?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Brand with ID '{brandId}' not found."),
                () => _mockBrandRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_BrandHasAssociatedProducts_ThrowsDuplicateDataException()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandDeleteCommand { Id = brandId };
            var brand = new Brand
            {
                Id = brandId,
                Name = "Nike",
                LogoUrl = "nike.png"
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(5)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("cannot be deleted because it is associated with 5 product(s)"),
                () => _mockBrandRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_FileStorageThrowsException_LogsWarningAndDoesNotThrow()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandDeleteCommand { Id = brandId };
            var brand = new Brand
            {
                Id = brandId,
                Name = "Under Armour",
                LogoUrl = "ua-logo.png"
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(brand)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.RemoveAsync(brandId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync(brand.LogoUrl, "brands", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Simulated disk error"))
                .Verifiable();

            // Act - should NOT throw despite file storage failure
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockBrandRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.VerifyAll()
            );
        }
    }
}

