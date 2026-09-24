using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Customers.Commands;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Customers.Commands
{
    [TestFixture]
    public class CustomerAddCommandHandlerTests
    {
        private AutoMock _mock;
        private CustomerAddCommandHandler _handler;
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
            _handler = _mock.Create<CustomerAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UniqueSelfCustomer_AddsCustomerWithAddress()
        {
            // Arrange
            var command = new CustomerAddCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+8801700000000",
                Origin = CustomerOrigin.Self,
                StreetAddress = "123 Main St",
                City = "Dhaka",
                Country = "Bangladesh"
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.IsDuplicateEmail(command.Email, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetByEmailAsync(command.Email.ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
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
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.FirstName.ShouldBe("John"),
                () => result.Email.ShouldBe("john.doe@example.com"),
                () => result.Addresses.Count.ShouldBe(1),
                () => result.Addresses.First().Address.City.ShouldBe("Dhaka"),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateEmailForSelfCustomer_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new CustomerAddCommand
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com",
                Origin = CustomerOrigin.Self
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.IsDuplicateEmail(command.Email, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("already exists"),
                () => _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}
