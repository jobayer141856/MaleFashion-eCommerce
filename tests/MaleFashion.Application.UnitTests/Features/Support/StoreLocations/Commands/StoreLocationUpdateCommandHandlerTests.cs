using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.StoreLocations.Commands;
using MaleFashion.Domain.Entities.Support;
using MapsterMapper;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.StoreLocations.Commands
{
    [TestFixture]
    public class StoreLocationUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private StoreLocationUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IStoreLocationRepository> _mockStoreLocationRepository;
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
            _mockStoreLocationRepository = _mock.Mock<IStoreLocationRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<StoreLocationUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockStoreLocationRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingLocation_UpdatesSuccessfully()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var location = new StoreLocation
            {
                Id = locationId,
                Title = "Old Title"
            };

            var command = new StoreLocationUpdateCommand
            {
                Id = locationId,
                Title = "New Title",
                Address = "New Address",
                PhoneNumber = "+1 000 000",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.IsDuplicateTitle(command.Title, locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(location)
                .Verifiable();

            _mockMapper.Setup(x => x.Map(command, location))
                .Returns(location)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.EditAsync(location, It.IsAny<CancellationToken>()))
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
                () => _mockStoreLocationRepository.VerifyAll(),
                () => _mockMapper.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateTitle_ThrowsDuplicateDataException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var command = new StoreLocationUpdateCommand { Id = locationId, Title = "Duplicate Title" };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.IsDuplicateTitle(command.Title, locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_LocationNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var command = new StoreLocationUpdateCommand { Id = locationId, Title = "New Title" };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.IsDuplicateTitle(command.Title, locationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
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

