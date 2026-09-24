using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.StoreLocations.Queries
{
    [TestFixture]
    public class GetStoreLocationByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetStoreLocationByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetStoreLocationByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockStoreLocationRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingLocation_ReturnsLocation()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var query = new GetStoreLocationByIdQuery { Id = locationId };
            var location = new StoreLocation
            {
                Id = locationId,
                Title = "Downtown Flagship"
            };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(locationId),
                () => result!.Title.ShouldBe("Downtown Flagship"),
                () => _mockStoreLocationRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_LocationNotFound_ReturnsNull()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var query = new GetStoreLocationByIdQuery { Id = locationId };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoreLocation?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockStoreLocationRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

