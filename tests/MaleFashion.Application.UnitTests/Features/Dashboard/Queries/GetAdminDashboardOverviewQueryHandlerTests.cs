using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Dashboard.DTOs;
using MaleFashion.Application.Features.Dashboard.Queries;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Dashboard.Queries
{
    [TestFixture]
    public class GetAdminDashboardOverviewQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAdminDashboardOverviewQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IOrderRepository> _mockOrderRepository;

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
            _mockOrderRepository = _mock.Mock<IOrderRepository>();
            _handler = _mock.Create<GetAdminDashboardOverviewQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOrderRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsDashboardOverview()
        {
            // Arrange
            var query = new GetAdminDashboardOverviewQuery();
            var overview = new AdminDashboardDto
            {
                TotalRevenue = 125000.50m,
                TotalOrders = 350,
                TotalCustomers = 120,
                TotalProducts = 85
            };

            _mockUnitOfWork.SetupGet(x => x.OrderRepository)
                .Returns(_mockOrderRepository.Object)
                .Verifiable();

            _mockOrderRepository.Setup(x => x.GetDashboardOverviewAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(overview)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.TotalRevenue.ShouldBe(125000.50m),
                () => result.TotalOrders.ShouldBe(350),
                () => result.TotalCustomers.ShouldBe(120),
                () => result.TotalProducts.ShouldBe(85),
                () => _mockOrderRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

