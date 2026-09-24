using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Partners.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Partners.Queries
{
    [TestFixture]
    public class GetPartnerClientByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetPartnerClientByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetPartnerClientByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockPartnerClientRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingPartner_ReturnsPartner()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var query = new GetPartnerClientByIdQuery { Id = partnerId };
            var partner = new PartnerClient
            {
                Id = partnerId,
                ClientName = "Acme Corp"
            };

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetByIdAsync(partnerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(partner)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(partnerId),
                () => result!.ClientName.ShouldBe("Acme Corp"),
                () => _mockPartnerClientRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_PartnerNotFound_ReturnsNull()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var query = new GetPartnerClientByIdQuery { Id = partnerId };

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetByIdAsync(partnerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartnerClient?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockPartnerClientRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

