using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.ContactInquiries.Commands;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.ContactInquiries.Commands
{
    [TestFixture]
    public class ContactInquiryToggleResolvedCommandHandlerTests
    {
        private AutoMock _mock;
        private ContactInquiryToggleResolvedCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IContactInquiryRepository> _mockContactInquiryRepository;

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
            _handler = _mock.Create<ContactInquiryToggleResolvedCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockContactInquiryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingInquiry_TogglesResolvedStatus()
        {
            // Arrange
            var inquiryId = Guid.NewGuid();
            var inquiry = new ContactInquiry
            {
                Id = inquiryId,
                Name = "Customer",
                IsResolved = false
            };

            var command = new ContactInquiryToggleResolvedCommand { Id = inquiryId };

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.GetByIdAsync(inquiryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inquiry)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.EditAsync(inquiry, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldBeTrue(),
                () => inquiry.IsResolved.ShouldBeTrue(),
                () => _mockContactInquiryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_InquiryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inquiryId = Guid.NewGuid();
            var command = new ContactInquiryToggleResolvedCommand { Id = inquiryId };

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.GetByIdAsync(inquiryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ContactInquiry?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

