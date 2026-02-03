using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore;
using AssetFox.Core.UnitTestsCore.Extensions;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCoreTests.Helpers;
using AssetFoxCoreTests.Tests.Announcement;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class AnnouncementTests
    {
        private AnnouncementController CreateController(Mock<IUnitOfWork> unitOfWork)
        {
            var security = EsecSecurityMocks.AdminMock;
            var hubService = HubServiceMocks.New();
            var contextAccessor = HttpContextAccessorMocks.DefaultMock();
            var controller = new AnnouncementController(security.Object,
                unitOfWork.Object,
                hubService.Object,
                contextAccessor.Object);
            return controller;
        }
        [Fact]
        public async Task ShouldReturnOkResultOnGet()
        {
            // Act
            var unitOfWork = UnitOfWorkMocks.New();
            var announcementRepository = AnnouncementRepositoryMocks.New(unitOfWork);
            var userRepo = UserRepositoryMocks.EveryoneExists(unitOfWork);
            var controller = CreateController(unitOfWork);
            var result = await controller.Announcements();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var _ = announcementRepository.SingleInvocationWithName(nameof(IAnnouncementRepository.Announcements));
        }

        [Fact]
        public async Task ShouldReturnOkResultOnPost()
        {
            // Act
            var dto = AnnouncementDtos.Dto();
            var unitOfWork = UnitOfWorkMocks.New();
            var announcementRepository = AnnouncementRepositoryMocks.New(unitOfWork);
            var userRepo = UserRepositoryMocks.EveryoneExists(unitOfWork);
            var controller = CreateController(unitOfWork);
            var result = await controller.UpsertAnnouncement(dto);

            // Assert
            Assert.IsType<OkResult>(result);
            var invocation = announcementRepository.SingleInvocationWithName(nameof(IAnnouncementRepository.UpsertAnnouncement));
            var argument = invocation.Arguments[0];
            ObjectAssertions.Equivalent(dto, argument);
        }

        [Fact]
        public async Task ShouldReturnOkResultOnDelete()
        {
            // Act

            var unitOfWork = UnitOfWorkMocks.New();
            var announcementRepository = AnnouncementRepositoryMocks.New(unitOfWork);
            var userRepo = UserRepositoryMocks.EveryoneExists(unitOfWork);
            var controller = CreateController(unitOfWork);
            var announcementId = Guid.NewGuid();
            var result = await controller.DeleteAnnouncement(announcementId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ShouldGetAllAnnouncements()
        {
            // Arrange
            var announcementId = Guid.NewGuid();
            var announcement = AnnouncementDtos.Dto(announcementId);
            var announcements = new List<AnnouncementDTO> { announcement };
            var unitOfWork = UnitOfWorkMocks.New();
            var users = UserRepositoryMocks.EveryoneExists(unitOfWork);
            var announcementRepository = AnnouncementRepositoryMocks.New(unitOfWork);
            announcementRepository.Setup(a => a.Announcements()).Returns(announcements);
            var controller = CreateController(unitOfWork);

            // Act
            var result = await controller.Announcements();

            // Assert
            var okObjResult = result as OkObjectResult;
            var dtos = (List<AnnouncementDTO>)Convert.ChangeType(okObjResult.Value, typeof(List<AnnouncementDTO>));
            Assert.True(dtos.Any());
            Assert.Equal(announcementId, dtos[0].Id);
        }

    }
}
