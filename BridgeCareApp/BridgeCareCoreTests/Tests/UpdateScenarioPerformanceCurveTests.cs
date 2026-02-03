using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Models;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Extensions;
using AssetFox.Core.UnitTestsCore.Tests;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Models;
using AssetFoxCore.Services;
using AssetFoxCoreTests.Helpers;
using AssetFoxCoreTests.Tests.General_Work_Queue;
using Moq;
using MoreLinq;
using Xunit;
using Assert = Xunit.Assert;

namespace AssetFoxCoreTests.Tests
{
    public class UpdateScenarioPerformanceCurveTests
    {
        private PerformanceCurveController CreateController(Mock<IUnitOfWork> unitOfWork)
        {
            var security = EsecSecurityMocks.AdminMock;
            var hubService = HubServiceMocks.New();
            var accessor = HttpContextAccessorMocks.DefaultMock();
            var claimHelper = ClaimHelperMocks.New();
            var expressionValidationService = ExpressionValidationServiceMocks.EverythingIsValid();
            var performanceCurvesService = new PerformanceCurvesService(unitOfWork.Object, hubService.Object, expressionValidationService.Object);
            var pagingService = new PerformanceCurvesPagingService(unitOfWork.Object);
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new PerformanceCurveController(
                security.Object,
                unitOfWork.Object,
                hubService.Object,
                accessor.Object,
                performanceCurvesService,
                pagingService,
                claimHelper.Object,
                generalWorkQueue.Object
                );
            return controller;
        }

        [Fact]
        public async Task UpsertScenarioPerformanceCurves_CurveUpdateInUpsert_PassesToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var performanceCurveRepo = PerformanceCurveRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var performanceCurveDto1 = PerformanceCurveDtos.Dto(curveId, libraryId);
            var performanceCurveDto2 = PerformanceCurveDtos.Dto(curveId, libraryId);
            performanceCurveRepo.Setup(p => p.GetScenarioPerformanceCurvesOrderedById(simulationId)).ReturnsList(performanceCurveDto1);
            performanceCurveDto2.Shift = true;

            var request = new PagingSyncModel<PerformanceCurveDTO>()
            {
                UpdateRows = new List<PerformanceCurveDTO> { performanceCurveDto2 },
                AddedRows = new List<PerformanceCurveDTO>(),
                RowsForDeletion = new List<Guid>()
            };

            // Act
            await controller.UpsertScenarioPerformanceCurves(simulationId, request);

            // assert
            var upsertCall = performanceCurveRepo.SingleInvocationWithName(nameof(IPerformanceCurveRepository.SaveScenarioPerformanceCurveChanges));
            var expectedArgument = new List<PerformanceCurveDTO> { performanceCurveDto2 };
            ObjectAssertions.Equivalent(expectedArgument, (upsertCall.Arguments[0] as UpsertAndDeleteModel<PerformanceCurveDTO>).UpdateRows);
            Assert.Equal(simulationId, upsertCall.Arguments[1]);
        }

        [Fact]
        public async Task UpsertScenarioPerformanceCurves_CurveDeletionInUpsert_PassesToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var performanceCurveRepo = PerformanceCurveRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var performanceCurveDto1 = PerformanceCurveDtos.Dto(curveId, libraryId);
            performanceCurveRepo.Setup(p => p.GetScenarioPerformanceCurvesOrderedById(simulationId)).ReturnsList(performanceCurveDto1);

            var request = new PagingSyncModel<PerformanceCurveDTO>()
            {
                UpdateRows = new List<PerformanceCurveDTO>(),
                AddedRows = new List<PerformanceCurveDTO>(),
                RowsForDeletion = new List<Guid> { curveId },
            };

            // Act
            await controller.UpsertScenarioPerformanceCurves(simulationId, request);

            // assert
            var upsertCall = performanceCurveRepo.SingleInvocationWithName(nameof(IPerformanceCurveRepository.SaveScenarioPerformanceCurveChanges));
            var expectedArgument = new List<PerformanceCurveDTO>();
            ObjectAssertions.Equivalent(expectedArgument, (upsertCall.Arguments[0] as UpsertAndDeleteModel<PerformanceCurveDTO>).UpdateRows);
            ObjectAssertions.Equivalent(expectedArgument, (upsertCall.Arguments[0] as UpsertAndDeleteModel<PerformanceCurveDTO>).AddedRows);
            Assert.Equal(simulationId, upsertCall.Arguments[1]);
        }

        [Fact]
        public async Task UpsertSimulationPerformanceCurve_AddInRequest_PassesToRepo()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var performanceCurveRepo = PerformanceCurveRepositoryMocks.New(unitOfWork);
            var controller = CreateController(unitOfWork);
            var simulationId = Guid.NewGuid();
            var curveId = Guid.NewGuid();
            var libraryId = Guid.NewGuid();
            var performanceCurveDto = PerformanceCurveDtos.Dto(curveId, libraryId);
            performanceCurveRepo.Setup(p => p.GetScenarioPerformanceCurvesOrderedById(simulationId)).ReturnsEmptyList();

            var request = new PagingSyncModel<PerformanceCurveDTO>()
            {
                AddedRows = new List<PerformanceCurveDTO> { performanceCurveDto },
                UpdateRows = new List<PerformanceCurveDTO>(),
                RowsForDeletion = new List<Guid>()
            };

            // Act
            await controller.UpsertScenarioPerformanceCurves(simulationId, request);

            // assert
            var upsertCall = performanceCurveRepo.SingleInvocationWithName(nameof(IPerformanceCurveRepository.SaveScenarioPerformanceCurveChanges));
            var expectedArgument = new List<PerformanceCurveDTO> { performanceCurveDto };
            ObjectAssertions.Equivalent(expectedArgument, (upsertCall.Arguments[0] as UpsertAndDeleteModel<PerformanceCurveDTO>).AddedRows);
            Assert.Equal(simulationId, upsertCall.Arguments[1]);
        }
    }
}
