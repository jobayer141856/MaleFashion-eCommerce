using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Customers.Commands;
using MaleFashion.Domain.Entities.Customers;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Customers.Commands
{
    [TestFixture]
    public class CustomerDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private CustomerDeleteCommandHandler _handler;
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
            _handler = _mock.Create<CustomerDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockCustomerRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_CustomerExists_DeletesCustomerSuccessfully()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new CustomerDeleteCommand { Id = customerId };
            var customer = new Customer { Id = customerId, FirstName = "Jane" };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.RemoveAsync(customerId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => _mockCustomerRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var command = new CustomerDeleteCommand { Id = customerId };

            _mockUnitOfWork.SetupGet(x => x.CustomerRepository)
                .Returns(_mockCustomerRepository.Object)
                .Verifiable();

            _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Customer with ID '{customerId}' not found."),
                () => _mockCustomerRepository.Verify(x => x.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

