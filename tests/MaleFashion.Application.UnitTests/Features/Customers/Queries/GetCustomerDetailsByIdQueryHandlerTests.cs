using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Features.Customers.Queries;
using MaleFashion.Domain.Entities.Customers;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Customers.Queries
{
    [TestFixture]
    public class GetCustomerDetailsByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetCustomerDetailsByIdQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ICustomerRepository> _mockCustomerRepository;

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
            _mockCustomerRepository = _mock.Mock<ICustomerRepository>();
            _handler = _mock.Create<GetCustomerDetailsByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CustomerExists_ReturnsCustomerDetails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerDetailsByIdQuery { Id = customerId };
            var customer = new Customer
            {
                Id = customerId,
                FirstName = "Bob",
                LastName = "Builder"
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerDetailsById(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(customerId),
                () => result!.FirstName.ShouldBe("Bob"),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CustomerDoesNotExist_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var query = new GetCustomerDetailsByIdQuery { Id = customerId };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerDetailsById(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeNull(),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

