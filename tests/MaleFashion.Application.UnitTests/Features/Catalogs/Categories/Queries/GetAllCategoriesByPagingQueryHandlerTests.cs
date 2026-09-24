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
    public class GetAllCategoriesByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllCategoriesByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllCategoriesByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCategoryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedCategories()
        {
            // Arrange
            var query = new GetAllCategoriesByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Shoes"
            };

            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Formal Shoes", IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Sports Shoes", IsActive = true }
            };

            _mockUnitOfWork.SetupGet(x => x.CategoryRepository)
                .Returns(_mockCategoryRepository.Object)
                .Verifiable();

            _mockCategoryRepository.Setup(x => x.GetPagedCategories(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IList<Category>)categories, 2, 2))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockCategoryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

