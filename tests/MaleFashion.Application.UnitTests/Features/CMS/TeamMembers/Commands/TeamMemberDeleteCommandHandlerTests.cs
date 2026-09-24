using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Services;
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
    public class TeamMemberDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private TeamMemberDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITeamMemberRepository> _mockTeamMemberRepository;
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
            _mockTeamMemberRepository = _mock.Mock<ITeamMemberRepository>();
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _handler = _mock.Create<TeamMemberDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockTeamMemberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingMember_DeletesMemberAndPhoto()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var member = new TeamMember
            {
                Id = memberId,
                Name = "Team Member",
                Role = "Designer",
                PhotoUrl = "photo.jpg"
            };

            var command = new TeamMemberDeleteCommand { Id = memberId };

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetByIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(member)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.RemoveAsync(memberId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync("photo.jpg", "team", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTeamMemberRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
            _mockFileStorageService.VerifyAll();
        }

        [Test]
        public async Task Handle_MemberNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var command = new TeamMemberDeleteCommand { Id = memberId };

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

