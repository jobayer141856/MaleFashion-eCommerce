using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.TeamMembers.Commands;
using MaleFashion.Domain.Entities.CMS;
using MapsterMapper;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.TeamMembers.Commands
{
    [TestFixture]
    public class TeamMemberAddCommandHandlerTests
    {
        private AutoMock _mock;
        private TeamMemberAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITeamMemberRepository> _mockTeamMemberRepository;
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
            _mockTeamMemberRepository = _mock.Mock<ITeamMemberRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<TeamMemberAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockTeamMemberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsTeamMemberSuccessfully()
        {
            // Arrange
            var command = new TeamMemberAddCommand
            {
                Name = "John Smith",
                Role = "Creative Director",
                PhotoUrl = "john.jpg"
            };

            var member = new TeamMember
            {
                Name = command.Name,
                Role = command.Role,
                PhotoUrl = command.PhotoUrl
            };

            _mockMapper.Setup(x => x.Map<TeamMember>(command))
                .Returns(member)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.AddAsync(member, It.IsAny<CancellationToken>()))
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
                () => result.Name.ShouldBe("John Smith"),
                () => result.Role.ShouldBe("Creative Director"),
                () => _mockMapper.VerifyAll(),
                () => _mockTeamMemberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

