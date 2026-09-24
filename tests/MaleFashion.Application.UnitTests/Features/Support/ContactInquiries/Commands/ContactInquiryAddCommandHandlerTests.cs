using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.ContactInquiries.Commands;
using MaleFashion.Domain.Entities.Support;
using MapsterMapper;
using Moq;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.ContactInquiries.Commands
{
    [TestFixture]
    public class ContactInquiryAddCommandHandlerTests
    {
        private AutoMock _mock;
        private ContactInquiryAddCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IContactInquiryRepository> _mockContactInquiryRepository;
        private Mock<IMapper> _mockMapper;

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
            _mockContactInquiryRepository = _mock.Mock<IContactInquiryRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<ContactInquiryAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockContactInquiryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommand_AddsContactInquirySuccessfully()
        {
            // Arrange
            var command = new ContactInquiryAddCommand
            {
                Name = "John Doe",
                Email = "john@example.com",
                Message = "Need help with sizing."
            };

            var inquiry = new ContactInquiry
            {
                Name = command.Name,
                Email = command.Email,
                Message = command.Message
            };

            _mockMapper.Setup(x => x.Map<ContactInquiry>(command))
                .Returns(inquiry)
                .Verifiable();

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.AddAsync(inquiry, It.IsAny<CancellationToken>()))
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
                () => result.Name.ShouldBe("John Doe"),
                () => result.Email.ShouldBe("john@example.com"),
                () => result.IsResolved.ShouldBeFalse(),
                () => _mockMapper.VerifyAll(),
                () => _mockContactInquiryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

