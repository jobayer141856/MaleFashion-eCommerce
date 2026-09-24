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
    public class BrandAddCommandHandlerTests
    {
        private AutoMock _mock;
        private BrandAddCommandHandler _handler;
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
            _handler = _mock.Create<BrandAddCommandHandler>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockMapper?.Reset();
            _mockBrandRepository?.Reset();
            _mockUnitOfWork?.Reset();
        }

        [Test]
        public async Task Handle_UniqueBrandName_AddsBrandAndReturnsBrand()
        {
            // Arrange
            var command = new BrandAddCommand
            {
                Name = "Nike",
                LogoUrl = "nike.png",
                IsActive = true
            };

            var brand = new Brand
            {
                Name = command.Name,
                LogoUrl = command.LogoUrl,
                IsActive = command.IsActive
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.IsDuplicateBrandName(command.Name, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .Verifiable();

            _mockMapper.Setup(x => x.Map<Brand>(command))
                .Returns(brand)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.AddAsync(brand, It.IsAny<CancellationToken>()))
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
                () => result.Id.ShouldNotBe(Guid.Empty),
                () => result.Name.ShouldBe(command.Name),
                () => result.LogoUrl.ShouldBe(command.LogoUrl),
                () => result.IsActive.ShouldBe(command.IsActive),
                () => _mockBrandRepository.VerifyAll(),
                () => _mockUnitOfWork.VerifyAll(),
                () => _mockMapper.VerifyAll()
            );
        }

        [Test]
        public async Task Handle_DuplicateBrandName_ThrowsDuplicateDataException()
        {
            // Arrange
            var command = new BrandAddCommand
            {
                Name = "Adidas",
                LogoUrl = "adidas.png",
                IsActive = true
            };

            _mockUnitOfWork.SetupGet(x => x.BrandRepository)
                .Returns(_mockBrandRepository.Object)
                .Verifiable();

            _mockBrandRepository.Setup(x => x.IsDuplicateBrandName(command.Name, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Verifiable();

            // Act & Assert
            var exception = await Should.ThrowAsync<DuplicateDataException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            this.ShouldSatisfyAllConditions(
                () => exception.Message.ShouldBe("Brand name already exists."),
                () => _mockBrandRepository.Verify(x => x.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Never),
                () => _mockUnitOfWork.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never)
            );
        }
    }
}

