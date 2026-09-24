using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Customers.Commands;
using MaleFashion.Domain.Entities.Customers;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Customers.Commands
{
    [TestFixture]
    public class CustomerUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private CustomerUpdateCommandHandler _handler;
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
            _handler = _mock.Create<CustomerUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_UpdatesCustomerDetails()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new CustomerUpdateCommand
            {
                Id = customerId,
                FirstName = "UpdatedFirst",
                LastName = "UpdatedLast",
                Email = "updated@example.com",
                PhoneNumber = "+8801900000000"
            };

            var existingCustomer = new Customer
            {
                Id = customerId,
                FirstName = "OldFirst",
                LastName = "OldLast",
                Email = "old@example.com",
                PhoneNumber = "+8801800000000"
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.IsDuplicateEmail(command.Email, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerDetailsById(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result.FirstName.ShouldBe("UpdatedFirst"),
                () => result.LastName.ShouldBe("UpdatedLast"),
                () => result.Email.ShouldBe("updated@example.com"),
                () => result.UpdatedAt.ShouldNotBeNull(),
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateEmail_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new CustomerUpdateCommand
            {
                Id = Guid.NewGuid(),
                Email = "duplicate@example.com"
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.IsDuplicateEmail(command.Email, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldContain("already exists"),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new CustomerUpdateCommand
            {
                Id = customerId,
                Email = "valid@example.com"
            };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.IsDuplicateEmail(command.Email, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetCustomerDetailsById(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Customer with ID '{customerId}' not found."),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

