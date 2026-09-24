using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.Partners.Commands;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Partners.Commands
{
    [TestFixture]
    public class PartnerClientDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private PartnerClientDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IPartnerClientRepository> _mockPartnerClientRepository;
        private Mock<IFileStorageService> _mockFileStorageService;

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
            _mockPartnerClientRepository = _mock.Mock<IPartnerClientRepository>();
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _handler = _mock.Create<PartnerClientDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockPartnerClientRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPartner_DeletesPartnerAndLogo()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerClient
            {
                Id = partnerId,
                ClientName = "Partner",
                LogoUrl = "logo.png"
            };

            var command = new PartnerClientDeleteCommand { Id = partnerId };

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetByIdAsync(partnerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(partner)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.RemoveAsync(partnerId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync("logo.png", "partners", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockPartnerClientRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
            _mockFileStorageService.VerifyAll();
        }

        [Test]
        public async Task Handle_PartnerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var command = new PartnerClientDeleteCommand { Id = partnerId };

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetByIdAsync(partnerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartnerClient?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

