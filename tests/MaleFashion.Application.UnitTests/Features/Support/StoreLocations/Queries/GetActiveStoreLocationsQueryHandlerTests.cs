using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.StoreLocations.Queries
{
    [TestFixture]
    public class GetActiveStoreLocationsQueryHandlerTests
    {
        private AutoMock _mock;
        private GetActiveStoreLocationsQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IStoreLocationRepository> _mockStoreLocationRepository;

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
            _mockStoreLocationRepository = _mock.Mock<IStoreLocationRepository>();
            _handler = _mock.Create<GetActiveStoreLocationsQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockStoreLocationRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsActiveLocations()
        {
            // Arrange
            var query = new GetActiveStoreLocationsQuery();
            var locations = new List<StoreLocation>
            {
                new StoreLocation { Title = "Active Store 1", IsActive = true },
                new StoreLocation { Title = "Active Store 2", IsActive = true }
            };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetActiveStoreLocations(It.IsAny<CancellationToken>()))
                .ReturnsAsync(locations)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Count.ShouldBe(2),
                () => _mockStoreLocationRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

