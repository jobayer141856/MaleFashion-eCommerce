using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Partners.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Partners.Queries
{
    [TestFixture]
    public class GetAllPartnersByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllPartnersByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllPartnersByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockPartnerClientRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedPartners()
        {
            // Arrange
            var query = new GetAllPartnersByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Corp"
            };

            var partners = new List<PartnerClient>
            {
                new PartnerClient { ClientName = "Acme Corp" },
                new PartnerClient { ClientName = "Global Corp" }
            };

            (IList<PartnerClient> Items, int TotalCount, int FilteredCount) expected = (partners, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.PartnerClientRepository)
                .Returns(_mockPartnerClientRepository.Object)
                .Verifiable();

            _mockPartnerClientRepository.Setup(x => x.GetPagedPartners(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockPartnerClientRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

