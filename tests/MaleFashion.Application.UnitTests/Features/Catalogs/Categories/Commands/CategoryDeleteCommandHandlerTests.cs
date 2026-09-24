using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Categories.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Categories.Commands
{
    [TestFixture]
    public class CategoryDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private CategoryDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IFileStorageService> _mockFileStorageService;
        private Mock<ILogger<CategoryDeleteCommandHandler>> _mockLogger;

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
            _mockCategoryRepository = _mock.Mock<ICategoryRepository>();
            _mockProductRepository = _mock.Mock<IProductRepository>();
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _mockLogger = _mock.Mock<ILogger<CategoryDeleteCommandHandler>>();
            _handler = _mock.Create<CategoryDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockProductRepository?.Reset();
            _mockCategoryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CategoryExistsWithoutConflictsWithImage_DeletesCategoryAndImage()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryDeleteCommand { Id = categoryId };
            var category = new Category
            {
                Id = categoryId,
                Name = "Accessories",
                ImageName = "accessories.jpg"
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.RemoveAsync(categoryId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync(category.ImageName, "images", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryDeleteCommand { Id = categoryId };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Category with ID '{categoryId}' not found."),
                () => _mockCategoryRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_CategoryHasProducts_ThrowsDuplicateDataException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryDeleteCommand { Id = categoryId };
            var category = new Category { Id = categoryId, Name = "Clothing" };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(4)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("cannot be deleted because it is associated with 4 product(s)"),
                () => _mockCategoryRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_CategoryHasSubCategories_ThrowsDuplicateDataException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryDeleteCommand { Id = categoryId };
            var category = new Category { Id = categoryId, Name = "Men" };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("cannot be deleted because it has 2 sub-category(ies)"),
                () => _mockCategoryRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_FileStorageFails_LogsErrorAndDoesNotRethrow()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryDeleteCommand { Id = categoryId };
            var category = new Category
            {
                Id = categoryId,
                Name = "Bags",
                ImageName = "bags.jpg"
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ProductRepository)
                .Returns(_mockProductRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category)
                .Verifiable();

            _mockProductRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetCountAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.RemoveAsync(categoryId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync(category.ImageName, "images", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Simulated disk failure"))
                .Verifiable();

            // Act - Should not throw
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockProductRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockFileStorageService.VerifyAll()
            );
        }
    }
}

