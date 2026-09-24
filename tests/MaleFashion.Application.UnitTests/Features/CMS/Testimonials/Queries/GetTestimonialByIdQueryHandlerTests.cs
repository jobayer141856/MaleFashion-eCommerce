using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Testimonials.Queries;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Testimonials.Queries
{
    [TestFixture]
    public class GetTestimonialByIdQueryHandlerTests
    {
        private AutoMock _mock;
        private GetTestimonialByIdQueryHandler _handler;
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
            _handler = _mock.Create<GetTestimonialByIdQueryHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTestimonialRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingTestimonial_ReturnsTestimonial()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var query = new GetTestimonialByIdQuery { Id = testimonialId };
            var testimonial = new Testimonial
            {
                Id = testimonialId,
                AuthorName = "Michael Brown",
                Quote = "Great experience"
            };

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetByIdAsync(testimonialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testimonial)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            this.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNull(),
                () => result!.Id.ShouldBe(testimonialId),
                () => result!.AuthorName.ShouldBe("Michael Brown"),
                () => result!.Quote.ShouldBe("Great experience"),
                () => _mockTestimonialRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_TestimonialNotFound_ReturnsNull()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var query = new GetTestimonialByIdQuery { Id = testimonialId };

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetByIdAsync(testimonialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Testimonial?)null)
                .Verifiable();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
            _mockTestimonialRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
        }
    }
}

