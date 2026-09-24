using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.TeamMembers.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.TeamMembers.Queries
{
    [TestFixture]
    public class GetAllTeamMembersByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllTeamMembersByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllTeamMembersByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTeamMemberRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedTeamMembers()
        {
            // Arrange
            var query = new GetAllTeamMembersByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Designer"
            };

            var members = new List<TeamMember>
            {
                new TeamMember { Name = "Alice", Role = "Lead Designer" },
                new TeamMember { Name = "Bob", Role = "Junior Designer" }
            };

            (IList<TeamMember> Items, int TotalCount, int FilteredCount) expected = (members, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.TeamMemberRepository)
                .Returns(_mockTeamMemberRepository.Object)
                .Verifiable();

            _mockTeamMemberRepository.Setup(x => x.GetPagedTeamMembers(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockTeamMemberRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

