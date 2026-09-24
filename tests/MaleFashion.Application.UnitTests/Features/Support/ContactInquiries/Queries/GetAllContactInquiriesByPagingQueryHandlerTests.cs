using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.ContactInquiries.Queries;
using MaleFashion.Domain.Entities.Support;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Support.ContactInquiries.Queries
{
    [TestFixture]
    public class GetAllContactInquiriesByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllContactInquiriesByPagingQueryHandler _handler;
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
            _handler = _mock.Create<GetAllContactInquiriesByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockContactInquiryRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedContactInquiries()
        {
            // Arrange
            var query = new GetAllContactInquiriesByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Order"
            };

            var inquiries = new List<ContactInquiry>
            {
                new ContactInquiry { Name = "Alice", Message = "Order query 1" },
                new ContactInquiry { Name = "Bob", Message = "Order query 2" }
            };

            (IList<ContactInquiry> Items, int TotalCount, int FilteredCount) expected = (inquiries, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.ContactInquiryRepository)
                .Returns(_mockContactInquiryRepository.Object)
                .Verifiable();

            _mockContactInquiryRepository.Setup(x => x.GetPagedContactInquiries(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockContactInquiryRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}
