using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Categories.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Categories.Queries
{
    [TestFixture]
    public class GetAllCategoriesQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllCategoriesQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;

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
            _handler = _mock.Create<GetAllCategoriesQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCategoryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_NoFilters_ReturnsAllCategories()
        {
            // Arrange
            var query = new GetAllCategoriesQuery();
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Shirts", IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Pants", IsActive = false }
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(2),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_ActiveOnlyFilter_ReturnsOnlyActiveCategories()
        {
            // Arrange
            var query = new GetAllCategoriesQuery { ActiveOnly = true };
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Shirts", IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Pants", IsActive = false }
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result[0].Name.ShouldBe("Shirts"),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_SearchTextFilter_ReturnsMatchingCategories()
        {
            // Arrange
            var query = new GetAllCategoriesQuery { SearchText = "Denim" };
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Shirts", IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Denim Jeans", IsActive = true }
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(1),
                () => result[0].Name.ShouldBe("Denim Jeans"),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

