using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.TeamMembers.Commands;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.TeamMembers.Commands
{
    [TestFixture]
    public class TeamMemberUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private TeamMemberUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITeamMemberRepository> _mockTeamMemberRepository;

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
            _handler = _mock.Create<TeamMemberUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTeamMemberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingMember_UpdatesSuccessfully()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var member = new TeamMember
            {
                Id = memberId,
                Name = "Old Name",
                Role = "Old Role",
                PhotoUrl = "old.jpg"
            };

            var command = new TeamMemberUpdateCommand
            {
                Id = memberId,
                Name = "New Name",
                Role = "New Role",
                PhotoUrl = "new.jpg"
            };

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetByIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(member)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.EditAsync(member, It.IsAny<CancellationToken>()))
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
                () => result.Name.ShouldBe("New Name"),
                () => result.Role.ShouldBe("New Role"),
                () => result.PhotoUrl.ShouldBe("new.jpg"),
                () => _mockTeamMemberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_MemberNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var command = new TeamMemberUpdateCommand { Id = memberId, Name = "Name" };

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetByIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TeamMember?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

