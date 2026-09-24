using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Testimonials.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Testimonials.Queries
{
    [TestFixture]
    public class GetAllTestimonialsByPagingQueryHandlerTests
    {
        private AutoMock _mock;
        private GetAllTestimonialsByPagingQueryHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITestimonialRepository> _mockTestimonialRepository;

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
            _mockTestimonialRepository = _mock.Mock<ITestimonialRepository>();
            _handler = _mock.Create<GetAllTestimonialsByPagingQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTestimonialRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ReturnsPagedTestimonials()
        {
            // Arrange
            var query = new GetAllTestimonialsByPagingQuery
            {
                PageIndex = 1,
                PageSize = 10,
                SearchText = "Great"
            };

            var testimonials = new List<Testimonial>
            {
                new Testimonial { AuthorName = "User 1", Quote = "Great shoes" },
                new Testimonial { AuthorName = "User 2", Quote = "Great jacket" }
            };

            (IList<Testimonial> Items, int TotalCount, int FilteredCount) expected = (testimonials, 2, 2);

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetPagedTestimonials(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.Item1.Count.ShouldBe(2),
                () => result.Item2.ShouldBe(2),
                () => result.Item3.ShouldBe(2),
                () => _mockTestimonialRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }
    }
}

