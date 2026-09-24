using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.ContactInquiries.Queries;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.ContactInquiries.Queries
{
    [TestFixture]
    public class GetContactInquiryByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetContactInquiryByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetContactInquiryByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockContactInquiryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingInquiry_ReturnsInquiry()
        {
            // Arrange
            var inquiryId = Guid.NewGuid();
            var query = new GetContactInquiryByIdQuery { Id = inquiryId };
            var inquiry = new ContactInquiry
            {
                Id = inquiryId,
                Name = "John Doe",
                Email = "john@example.com"
            };

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.GetByIdAsync(inquiryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inquiry)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(inquiryId),
                () => result!.Name.ShouldBe("John Doe"),
                () => _mockContactInquiryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_InquiryNotFound_ReturnsNull()
        {
            // Arrange
            var inquiryId = Guid.NewGuid();
            var query = new GetContactInquiryByIdQuery { Id = inquiryId };

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.GetByIdAsync(inquiryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ContactInquiry?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockContactInquiryRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

