using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Services;
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
    public class TestimonialDeleteCommandHandlerTests
    {
        private AutoMock _mock;
        private TestimonialDeleteCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<ITestimonialRepository> _mockTestimonialRepository;
        private Mock<IFileStorageService> _mockFileStorageService;

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
            _mockFileStorageService = _mock.Mock<IFileStorageService>();
            _handler = _mock.Create<TestimonialDeleteCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockFileStorageService?.Reset();
            _mockTestimonialRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ExistingTestimonial_DeletesTestimonialAndAvatar()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var testimonial = new Testimonial
            {
                Id = testimonialId,
                AuthorName = "Client",
                Quote = "Great service",
                AvatarUrl = "avatar.jpg"
            };

            var command = new TestimonialDeleteCommand { Id = testimonialId };

            _mockUnitOfWork.SetupGet(x => x.TestimonialRepository)
                .Returns(_mockTestimonialRepository.Object)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.GetByIdAsync(testimonialId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testimonial)
                .Verifiable();

            _mockTestimonialRepository.Setup(x => x.RemoveAsync(testimonialId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockFileStorageService.Setup(x => x.DeleteImageAsync("avatar.jpg", "testimonials", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockTestimonialRepository.VerifyAll();
            _mockUnitOfWork.VerifyAll();
            _mockFileStorageService.VerifyAll();
        }

        [Test]
        public async Task Handle_TestimonialNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var testimonialId = Guid.NewGuid();
            var command = new TestimonialDeleteCommand { Id = testimonialId };

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

