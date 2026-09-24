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
    public class CategoryAddCommandHandlerTests
    {
        private AutoMock _mock;
        private CategoryAddCommandHandler _handler;
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
            _handler = _mock.Create<CategoryAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockCategoryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UniqueCategoryName_AddsCategoryAndReturnsCategory()
        {
            // Arrange
            var command = new CategoryAddCommand
            {
                Name = "Jackets",
                ImageName = "jackets.jpg",
                IsActive = true
            };

            var category = new Category
            {
                Name = command.Name,
                ImageName = command.ImageName,
                IsActive = command.IsActive
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.IsDuplicateCategoryName(command.Name, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockMapper.Setup(x => x.Map<Category>(command))
                .Returns(category)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.AddAsync(category, It.IsAny<CancellationToken>()))
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
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.Name.ShouldBe(command.Name),
                () => result.ImageName.ShouldBe(command.ImageName),
                () => result.IsActive.ShouldBe(command.IsActive),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockMapper.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateCategoryName_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new CategoryAddCommand
            {
                Name = "Shoes",
                ImageName = "shoes.jpg",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.IsDuplicateCategoryName(command.Name, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Category name already exists."),
                () => _mockCategoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

