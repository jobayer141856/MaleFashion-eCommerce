using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
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
    public class PartnerClientUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private PartnerClientUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IPartnerClientRepository> _mockPartnerClientRepository;

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
            _handler = _mock.Create<PartnerClientUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockPartnerClientRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPartner_UpdatesSuccessfully()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerClient
            {
                Id = partnerId,
                ClientName = "Old Name",
                LogoUrl = "old.png"
            };

            var command = new PartnerClientUpdateCommand
            {
                Id = partnerId,
                ClientName = "New Name",
                LogoUrl = "new.png"
            };

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetByIdAsync(partnerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(partner)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.EditAsync(partner, It.IsAny<CancellationToken>()))
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
                () => result.ClientName.ShouldBe("New Name"),
                () => result.LogoUrl.ShouldBe("new.png"),
                () => _mockPartnerClientRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_PartnerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var command = new PartnerClientUpdateCommand { Id = partnerId, ClientName = "Name" };

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

