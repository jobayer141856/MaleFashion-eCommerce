using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.Testimonials.Commands;
using MaleFashion.Domain.Entities.CMS;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.CMS.Testimonials.Commands
{
    [TestFixture]
    public class TestimonialUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private TestimonialUpdateCommandHandler _handler;
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
            _handler = _mock.Create<TestimonialUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockTestimonialRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingTestimonial_UpdatesSuccessfully()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var testimonial = new Testimonial
            {
                Id = testimonialId,
                AuthorName = "Old Client",
                Quote = "Old Feedback"
            };

            var command = new TestimonialUpdateCommand
            {
                Id = testimonialId,
                AuthorName = "New Client",
                AuthorProfession = "CEO",
                Quote = "Superb service!",
                AvatarUrl = "new.jpg"
            };

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetByIdAsync(testimonialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testimonial)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.EditAsync(testimonial, It.IsAny<CancellationToken>()))
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
                () => result.AuthorName.ShouldBe("New Client"),
                () => result.Quote.ShouldBe("Superb service!"),
                () => _mockTestimonialRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_TestimonialNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var command = new TestimonialUpdateCommand { Id = testimonialId, AuthorName = "Name" };

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetByIdAsync(testimonialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Testimonial?)null)
                .Verifiable();

            // Act & Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}

