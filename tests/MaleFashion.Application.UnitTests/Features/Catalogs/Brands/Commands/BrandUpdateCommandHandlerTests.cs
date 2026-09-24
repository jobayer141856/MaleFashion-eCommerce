using Autofac.Extras.Moq;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Brands.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using MapsterMapper;
using Moq;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.UnitTests.Features.Catalogs.Brands.Commands
{
    [TestFixture]
    public class BrandUpdateCommandHandlerTests
    {
        private AutoMock _mock;
        private BrandUpdateCommandHandler _handler;
        private Mock<IApplicationUnitOfWork> _mockUnitOfWork;
        private Mock<IBrandRepository> _mockBrandRepository;
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
            _mockBrandRepository = _mock.Mock<IBrandRepository>();
            _mockMapper = _mock.Mock<IMapper>();
            _handler = _mock.Create<BrandUpdateCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_ValidCommandAndUniqueName_UpdatesBrandAndReturnsBrand()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandUpdateCommand
            {
                Id = brandId,
                Name = "Puma Updated",
                LogoUrl = "puma-updated.png",
                IsActive = true
            };

            var existingBrand = new Brand
            {
                Id = brandId,
                Name = "Puma Old",
                LogoUrl = "puma-old.png",
                IsActive = false
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.IsDuplicateBrandName(command.Name, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetById(command.Id))
                .Returns(existingBrand)
                .Verifiable();

            _mockMapper.Setup(x => x.Map(command, existingBrand))
                .Returns(new Brand
                {
                    Id = brandId,
                    Name = command.Name,
                    LogoUrl = command.LogoUrl,
                    IsActive = command.IsActive
                })
                .Verifiable();

            _mockBrandRepository.Setup(x => x.EditAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
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
                () => result.Id.ShouldBe(brandId),
                () => result.Name.ShouldBe(command.Name),
                () => result.LogoUrl.ShouldBe(command.LogoUrl),
                () => result.IsActive.ShouldBeTrue(),
                () => result.UpdatedAt.ShouldNotBeNull(),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockMapper.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateBrandName_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new BrandUpdateCommand
            {
                Id = Guid.NewGuid(),
                Name = "ExistingBrand",
                LogoUrl = "logo.png",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.IsDuplicateBrandName(command.Name, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Brand name already exists."),
                () => _mockBrandRepository.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never),
                () => _mockBrandRepository.Verify(x => x.EditAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }

        [Test]
        public async Task Handle_BrandNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var brandId = Guid.NewGuid();
            var command = new BrandUpdateCommand
            {
                Id = brandId,
                Name = "NonExistentBrand",
                LogoUrl = "logo.png",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.IsDuplicateBrandName(command.Name, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.GetById(command.Id))
                .Returns((Brand)null!)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe($"Brand with ID '{brandId}' not found."),
                () => _mockBrandRepository.Verify(x => x.EditAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}
