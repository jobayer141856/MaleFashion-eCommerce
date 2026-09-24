using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.TeamMembers.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.TeamMembers.Queries
{
    [TestFixture]
    public class GetTeamMemberByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetTeamMemberByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetTeamMemberByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTeamMemberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingMember_ReturnsMember()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var query = new GetTeamMemberByIdQuery { Id = memberId };
            var member = new TeamMember
            {
                Id = memberId,
                Name = "Sarah Connor",
                Role = "Operations"
            };

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetByIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(member)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(memberId),
                () => result!.Name.ShouldBe("Sarah Connor"),
                () => result!.Role.ShouldBe("Operations"),
                () => _mockTeamMemberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_MemberNotFound_ReturnsNull()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var query = new GetTeamMemberByIdQuery { Id = memberId };

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetByIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TeamMember?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockTeamMemberRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

