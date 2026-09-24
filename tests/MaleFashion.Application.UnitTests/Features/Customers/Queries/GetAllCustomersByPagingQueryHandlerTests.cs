using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Features.Customers.Queries;
using MaleFashion.Domain.Entities.Customers;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Customers.Queries
{
    [TestFixture]
    public class GetAllCustomersByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllCustomersByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllCustomersByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedCustomers()
        {
            // Arrange
            var query = new GetAllCustomersByPagingQuery
            {
                Start = 0,
                Length = 10,
                SearchText = "Doe"
            };

            var customers = new List<Customer>
            {
                new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" }
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetPagedCustomers(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IList<Customer>)customers, 1, 1))
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(1),
                () => result.Item2.ShouldBe(1),
                () => result.Item3.ShouldBe(1),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

