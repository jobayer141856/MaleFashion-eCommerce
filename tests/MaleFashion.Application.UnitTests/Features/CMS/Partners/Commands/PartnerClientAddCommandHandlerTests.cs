using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Partners.Commands;
using MaleFashion.Domain.Entities.CMS;
using MapsterMapper;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Partners.Commands
{
    [TestFixture]
    public class PartnerClientAddCommandHandlerTests
    {
        private AutoMock _mock;
        private PartnerClientAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IPartnerClientRepository> _mockPartnerClientRepository;
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
            _mockPartnerClientRepository = _mock.Mock<IPartnerClientRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<PartnerClientAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockPartnerClientRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsPartnerSuccessfully()
        {
            // Arrange
            var command = new PartnerClientAddCommand
            {
                ClientName = "Acme Corp",
                LogoUrl = "acme.png"
            };

            var partner = new PartnerClient
            {
                ClientName = command.ClientName,
                LogoUrl = command.LogoUrl
            };

            _mockMapper.Setup(x => x.Map<PartnerClient>(command))
                .Returns(partner)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.AddAsync(partner, It.IsAny<CancellationToken>()))
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
                () => result.ClientName.ShouldBe("Acme Corp"),
                () => _mockMapper.VerifyAll(),
                () => _mockPartnerClientRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
