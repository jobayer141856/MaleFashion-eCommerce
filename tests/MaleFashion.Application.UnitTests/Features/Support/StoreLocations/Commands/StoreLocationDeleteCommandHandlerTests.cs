using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.StoreLocations.Commands;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.StoreLocations.Commands
{
    [TestFixture]
    public class StoreLocationDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private StoreLocationDeleteCommandHandler _handler;
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
            _handler = _mock.Create<StoreLocationDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockStoreLocationRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingLocation_DeletesSuccessfully()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var location = new StoreLocation { Id = locationId, Title = "Location" };
            var command = new StoreLocationDeleteCommand { Id = locationId };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.RemoveAsync(locationId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockStoreLocationRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }

        [Test]
        public async Task Handle_LocationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var command = new StoreLocationDeleteCommand { Id = locationId };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoreLocation?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

