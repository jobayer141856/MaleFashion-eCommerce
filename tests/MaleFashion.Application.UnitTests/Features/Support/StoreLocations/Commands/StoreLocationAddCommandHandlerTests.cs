using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.StoreLocations.Commands;
using MaleFashion.Domain.Entities.Support;
using MapsterMapper;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.StoreLocations.Commands
{
    [TestFixture]
    public class StoreLocationAddCommandHandlerTests
    {
        private AutoMock _mock;
        private StoreLocationAddCommandHandler _handler;
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
            _handler = _mock.Create<StoreLocationAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockStoreLocationRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UniqueTitle_AddsStoreLocationSuccessfully()
        {
            // Arrange
            var command = new StoreLocationAddCommand
            {
                Title = "Downtown Flagship",
                Address = "100 Fashion Street, NY",
                PhoneNumber = "+1 234 567 890",
                Email = "ny@malefashion.com",
                DisplayOrder = 1,
                IsActive = true
            };

            var location = new StoreLocation
            {
                Title = command.Title,
                Address = command.Address,
                PhoneNumber = command.PhoneNumber,
                Email = command.Email,
                DisplayOrder = command.DisplayOrder,
                IsActive = command.IsActive
            };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.IsDuplicateTitle(command.Title, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockMapper.Setup(x => x.Map<StoreLocation>(command))
                .Returns(location)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.AddAsync(location, It.IsAny<CancellationToken>()))
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
                () => result.Title.ShouldBe("Downtown Flagship"),
                () => _mockStoreLocationRepository.VerifyAll(),
                () => _mockMapper.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateTitle_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new StoreLocationAddCommand
            {
                Title = "Downtown Flagship",
                Address = "100 Fashion Street"
            };

            _mockUnitOfWork.SetupGet(x => x.StoreLocationRepository)
                .Returns(_mockStoreLocationRepository.Object)
                .Verifiable();

            _mockStoreLocationRepository.Setup(x => x.IsDuplicateTitle(command.Title, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

