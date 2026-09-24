using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Categories.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MapsterMapper;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Categories.Commands
{
    [TestFixture]
    public class CategoryUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private CategoryUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IMapper> _mockMapper;

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
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<CategoryUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockCategoryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommandAndUniqueName_UpdatesCategoryAndReturnsCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryUpdateCommand
            {
                Id = categoryId,
                Name = "Jackets & Coats",
                ImageName = "jackets-coats.jpg",
                IsActive = true
            };

            var existingCategory = new Category
            {
                Id = categoryId,
                Name = "Jackets",
                ImageName = "jackets.jpg",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.IsDuplicateCategoryName(command.Name, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetById(command.Id))
                .Returns(existingCategory)
                .Verifiable();

            _mockMapper.Setup(x => x.Map(command, existingCategory))
                .Returns(new Category
                {
                    Id = categoryId,
                    Name = command.Name,
                    ImageName = command.ImageName,
                    IsActive = command.IsActive
                })
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.EditAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.Id.ShouldBe(categoryId),
                () => result.Name.ShouldBe(command.Name),
                () => result.ImageName.ShouldBe(command.ImageName),
                () => result.UpdatedAt.ShouldNotBeNull(),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockMapper.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateCategoryName_ThrowsDuplicateDataException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CategoryUpdateCommand
            {
                Id = categoryId,
                Name = "ExistingCategory",
                ImageName = "test.jpg",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.IsDuplicateCategoryName(command.Name, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Category name is duplicate"),
                () => _mockCategoryRepository.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never),
                () => _mockCategoryRepository.Verify(x => x.EditAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

