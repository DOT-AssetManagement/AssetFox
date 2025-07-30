using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using BridgeCareCore.Controllers;
using BridgeCareCore.Models;
using BridgeCareCore.Services;
using BridgeCareCoreTests.Helpers;
using BridgeCareCoreTests.Tests.General_Work_Queue;
using Moq;
using Xunit;

namespace BridgeCareCoreTests.Tests.Integration
{
    public class PerformanceCurveControllerIntegrationTests
    {
        public PerformanceCurveController CreateController(Mock<IHubService> hubserviceMock = null)
        {
            var security = EsecSecurityMocks.Admin;
            hubserviceMock ??= HubServiceMocks.New();
            var contextAccessor = HttpContextAccessorMocks.Default();
            var claimHelper = ClaimHelperMocks.New();
            var expressionValidationService = ExpressionValidationServiceMocks.EverythingIsValid();
            var service = new PerformanceCurvesService(TestHelper.UnitOfWork, hubserviceMock.Object, expressionValidationService.Object);
            var pagingService = new PerformanceCurvesPagingService(TestHelper.UnitOfWork);
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new PerformanceCurveController(
                security,
                TestHelper.UnitOfWork,
                hubserviceMock.Object,
                contextAccessor,
                service,
                pagingService,
                claimHelper.Object,
                generalWorkQueue.Object);
            return controller;
        }

        [Fact]
        public async Task UpsertPerformanceCurveLibrary_CurveUpsertThrows_LibraryIsNotChanged()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var attributeName = TestAttributeNames.CulvDurationN;
            var hubService = HubServiceMocks.New();
            var controller = CreateController(hubService);
            var libraryId = Guid.NewGuid();
            var library = PerformanceCurveLibraryDtos.Empty(libraryId);
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertPerformanceCurveLibrary(library);
            var curveId = Guid.NewGuid();
            var criterionLibraryId = Guid.NewGuid();
            var curve = PerformanceCurveDtos.Dto(curveId, criterionLibraryId, attributeName);
            var curves = new List<PerformanceCurveDTO> { curve };
            TestHelper.UnitOfWork.PerformanceCurveRepo.UpsertOrDeletePerformanceCurves(curves, libraryId);
            var updateLibrary = PerformanceCurveLibraryDtos.Empty(libraryId);
            updateLibrary.Description = "Updated description";
            var updateCurve = PerformanceCurveDtos.Dto(curveId, criterionLibraryId, "AttributeDoesNotExist");
            var syncModel = new PagingSyncModel<PerformanceCurveDTO>
            {
                UpdateRows = new List<PerformanceCurveDTO> { updateCurve },
            };
            var upsertRequest = new LibraryUpsertPagingRequestModel<PerformanceCurveLibraryDTO, PerformanceCurveDTO>
            {
                Library = library,
                SyncModel = syncModel,
            };
            var libraryBefore = TestHelper.UnitOfWork.PerformanceCurveRepo.GetPerformanceCurveLibrary(libraryId);

            await controller.UpsertPerformanceCurveLibrary(upsertRequest);

            var message = hubService.GetSingleThreeArgumentErrorMessage();
            var libraryAfter = TestHelper.UnitOfWork.PerformanceCurveRepo.GetPerformanceCurveLibrary(libraryId);
            ObjectAssertions.Equivalent(libraryBefore, libraryAfter);
        }
    }
}
