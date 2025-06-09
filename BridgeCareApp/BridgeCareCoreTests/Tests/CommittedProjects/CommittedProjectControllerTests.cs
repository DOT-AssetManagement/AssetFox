using System.Data;
using System.Security.Claims;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Models;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using AppliedResearchAssociates.iAM.WorkQueue;
using BridgeCareCore.Controllers;
using BridgeCareCore.Interfaces;
using BridgeCareCore.Models;
using BridgeCareCore.Services;
using BridgeCareCore.Services.General_Work_Queue.WorkItems;
using BridgeCareCore.Utils;
using BridgeCareCore.Utils.Interfaces;
using BridgeCareCoreTests.Helpers;
using BridgeCareCoreTests.Tests.General_Work_Queue;
using BridgeCareCoreTests.Tests.Integration;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Moq;
using OfficeOpenXml;
using Xunit;
using Policy = BridgeCareCore.Security.SecurityConstants.Policy;

namespace BridgeCareCoreTests.Tests
{
    public class CommittedProjectControllerTests
    {
        private Mock<IUnitOfWork> _mockUOW;
        private Mock<ICommittedProjectService> _mockService;
        private Mock<ICommittedProjectPagingService> _mockPagingService;
        private Mock<ICommittedProjectRepository> _mockCommittedProjectRepo;
        private Guid _badScenario = Guid.Parse("0c66674c-8fcb-462b-8765-69d6815e0958");
        private readonly Mock<IClaimHelper> _mockClaimHelper = new();

        public CommittedProjectControllerTests()
        {
            _mockUOW = new Mock<IUnitOfWork>();
            // This is the DEFAULT current user (a user in the admin role)
            // It MUST be changed if testing for an unauthorized user.
            _mockUOW.Setup(_ => _.CurrentUser).Returns(UserDtos.Admin());

            var mockSimulationRepo = new Mock<ISimulationRepository>();
            _mockUOW.Setup(_ => _.SimulationRepo).Returns(mockSimulationRepo.Object);

            _mockCommittedProjectRepo = new Mock<ICommittedProjectRepository>();
            _mockUOW.Setup(_ => _.CommittedProjectRepo).Returns(_mockCommittedProjectRepo.Object);

            var mockUserRepo = UserRepositoryMocks.EveryoneExists(_mockUOW);

            _mockService = new Mock<ICommittedProjectService>();
            _mockService.Setup(_ => _.ExportCommittedProjectsFile(It.IsAny<Guid>()))
                .Returns(TestDataForCommittedProjects.GoodFile());
            _mockPagingService = new Mock<ICommittedProjectPagingService>();
        }

        public CommittedProjectController CreateTestController(List<string> userClaims)
        {
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var testUser = ClaimsPrincipals.WithNameClaims(userClaims);
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.AdminMock.Object,
                _mockUOW.Object,
                hubService,
                accessor,
                _mockClaimHelper.Object, generalWorkQueue.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = testUser }
            };
            return controller;
        }

        [Fact]
        public async Task ExportWorksOnValidUser()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act
            var result = await controller.ExportCommittedProjects(TestDataForCommittedProjects.SimulationId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var contents = okResult.Value as FileInfoDTO;
            Assert.Equal(TestDataForCommittedProjects.GoodFile().FileName, contents.FileName);
            Assert.True(contents.FileData.Length > 0);
        }

        [Fact]
        public async Task ImportFailsWithNoFile()
        {
            // Arrange
            var mockContextAccessor = new Mock<IHttpContextAccessor>();
            mockContextAccessor.Setup(_ => _.HttpContext)
                .Returns(CreateContextWithNoFile(TestDataForCommittedProjects.SimulationId));
            var hubService = HubServiceMocks.New();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService.Object,
                mockContextAccessor.Object, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act

            await controller.ImportCommittedProjects();

            // Assert
            var message = hubService.GetSingleThreeArgumentErrorMessage();
            Assert.Contains("Committed project file not found.", message);
            _mockService.Verify(_ => _.ImportCommittedProjectFiles(It.IsAny<Guid>(), It.IsAny<ExcelPackage>(), It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never());

        }

        [Fact]
        public async Task ImportFailsWithNoFormData()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.New();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Dbe,
                _mockUOW.Object,
                hubService.Object,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act
            await controller.ImportCommittedProjects();

            // Assert
            _mockService.Verify(_ => _.ImportCommittedProjectFiles(It.IsAny<Guid>(), It.IsAny<ExcelPackage>(), It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never());
            var message = hubService.GetSingleThreeArgumentErrorMessage();
            Assert.Contains(CommittedProjectController.RequestMimeTypeIsInvalid, message);
        }

        [Fact]
        public async Task ImportFailsWithNoMimeType()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.New();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService.Object,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act + Assert
            await controller.ImportCommittedProjects();

            var message = hubService.GetSingleThreeArgumentErrorMessage();
            Assert.Contains(CommittedProjectController.RequestMimeTypeIsInvalid, message);
        }


        [Fact]
        public async Task DeleteSimulationWorksWithValidSimulation()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act
            var result = await controller.DeleteSimulationCommittedProjects(TestDataForCommittedProjects.SimulationId);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockCommittedProjectRepo.Verify(_ => _.DeleteSimulationCommittedProjects(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task DeleteSpecificWorksWithValidProject()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor,
                _mockClaimHelper.Object,
                generalWorkQueue.Object);
            var deleteList = new List<Guid>()
            {
                TestDataForCommittedProjects.CommittedProjectId1,
                TestDataForCommittedProjects.CommittedProjectId2,
            };

            // Act
            var result = await controller.DeleteSpecificCommittedProjects(deleteList);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockCommittedProjectRepo.Verify(_ => _.DeleteSpecificCommittedProjects(It.IsAny<List<Guid>>()), Times.Once());
        }

        [Fact]
        public async Task DeleteSpecificHandlesBadProject()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);
            var deleteList = new List<Guid>()
            {
                _badScenario
            };

            // Act
            var result = await controller.DeleteSpecificCommittedProjects(deleteList);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetSectionWorksOnValidUser()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            _mockCommittedProjectRepo.Setup(_ => _.GetSectionCommittedProjectDTOs(It.IsAny<Guid>()))
                .Returns(TestDataForCommittedProjects.ValidCommittedProjects);
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act
            var result = await controller.GetCommittedProjects(TestDataForCommittedProjects.SimulationId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var contents = okResult.Value as List<SectionCommittedProjectDTO>;
            Assert.Equal(3, contents.Count);
        }

        [Fact]
        public async Task GetSectionHandlesBadScenario()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            _mockCommittedProjectRepo.Setup(_ => _.GetSectionCommittedProjectDTOs(It.IsAny<Guid>()))
                .Throws<RowNotInTableException>();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            // Act
            var result = await controller.GetCommittedProjects(_badScenario);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpsertSectionWorksWithValidProjects()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);

            var sync = new PagingSyncModel<SectionCommittedProjectDTO>()
            {
                LibraryId = null,
                AddedRows = TestDataForCommittedProjects.ValidCommittedProjects,
                UpdateRows = new List<SectionCommittedProjectDTO>(),
                RowsForDeletion = new List<Guid>()
            };

            // Act
            var result = await controller.UpsertCommittedProjects(TestDataForCommittedProjects.ValidCommittedProjects[0].SimulationId, sync);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockCommittedProjectRepo.Verify(_ => _.SaveCommittedProjectChanges(It.IsAny<UpsertAndDeleteModel<SectionCommittedProjectDTO>>(), It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task UpsertHandlesBadScenario()
        {
            // Arrange
            var accessor = HttpContextAccessorMocks.Default();
            var hubService = HubServiceMocks.Default();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new CommittedProjectController(
                _mockService.Object,
                _mockPagingService.Object,
                EsecSecurityMocks.Admin,
                _mockUOW.Object,
                hubService,
                accessor, _mockClaimHelper.Object, generalWorkQueue.Object);
            _mockCommittedProjectRepo.Setup(_ => _.SaveCommittedProjectChanges(It.IsAny<UpsertAndDeleteModel<SectionCommittedProjectDTO>>(), It.IsAny<Guid>()))
                .Throws<RowNotInTableException>();

            var sync = new PagingSyncModel<SectionCommittedProjectDTO>()
            {
                LibraryId = null,
                AddedRows = TestDataForCommittedProjects.ValidCommittedProjects,
                UpdateRows = new List<SectionCommittedProjectDTO>(),
                RowsForDeletion = new List<Guid>()
            };


            // Act
            var result = await controller.UpsertCommittedProjects(TestDataForCommittedProjects.ValidCommittedProjects[0].SimulationId, sync);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task UserIsModifyCommittedProjectsAuthorized()
        {
            // non-admin unauthorize test
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policy.ModifyCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectModifyAnyAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var controller = CreateTestController(roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Editor }));
            // Act
            var allowed = await authorizationService.AuthorizeAsync(controller.User, Policy.ModifyCommittedProjects);
            // Assert
            Assert.False(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsViewCommittedAuthorized()
        {
            // non-admin authorize test
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policy.ViewCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewAnyAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewPermittedAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var controller = CreateTestController(roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Editor }));
            // Act
            var allowed = await authorizationService.AuthorizeAsync(controller.User, Policy.ViewCommittedProjects);
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsImportCommittedProjectsAuthorized()
        {
            // admin authorize test
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policy.ImportCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectImportAnyAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectImportPermittedAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var controller = CreateTestController(roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.Esec, new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Administrator }));
            // Act
            var allowed = await authorizationService.AuthorizeAsync(controller.User, Policy.ImportCommittedProjects);
            // Assert
            Assert.True(allowed.Succeeded);
        }
        [Fact]
        public async Task UserIsViewCommittedAuthorized_B2C()
        {
            // non-admin authorize test
            // Arrange
            var authorizationService = BuildAuthorizationServiceMocks.BuildAuthorizationService(services =>
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(Policy.ViewCommittedProjects,
                        policy => policy.RequireClaim(ClaimTypes.Name,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewAnyAccess,
                                                      BridgeCareCore.Security.SecurityConstants.Claim.CommittedProjectViewPermittedAccess));
                });
            });
            var roleClaimsMapper = new RoleClaimsMapper();
            var controller = CreateTestController(roleClaimsMapper.GetClaims(BridgeCareCore.Security.SecurityConstants.SecurityTypes.B2C, new List<string> { BridgeCareCore.Security.SecurityConstants.Role.Administrator }));
            // Act
            var allowed = await authorizationService.AuthorizeAsync(controller.User, Policy.ViewCommittedProjects);
            // Assert
            Assert.True(allowed.Succeeded);
        }

        #region Helpers
        private UserDTO UnauthorizedUser => new UserDTO
        {
            Username = "Nonadmin",
            HasInventoryAccess = true,
            Id = TestDataForCommittedProjects.UnauthorizedUser
        };

        #endregion

        private HttpContext CreateContextWithNoFile(Guid simulationId)
        {
            var httpContext = new DefaultHttpContext();
            HttpContextSetup.AddAuthorizationHeader(httpContext);
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data");

            var formData = new Dictionary<string, StringValues>()
            {
                {"applyNoTreatment", new StringValues("0")},
                {"simulationId", new StringValues(simulationId.ToString())}
            };

            httpContext.Request.Form = new FormCollection(formData);
            return httpContext;
        }
    }
}
