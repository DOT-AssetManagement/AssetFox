using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services;
using AssetFoxCore.Services.Treatment;
using AssetFoxCoreTests.Tests.General_Work_Queue;
using AssetFoxCoreTests.Tests.Treatment;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class TestTreatmentControllerSetup
    {
        public static TreatmentController Create(
            Mock<IUnitOfWork> unitOfWork,
            Mock<ITreatmentPagingService> pagingService = null
            )
        {
            var contextAccessor = HttpContextAccessorMocks.DefaultMock();
            var hubService = HubServiceMocks.New();
            var esecSecurity = EsecSecurityMocks.AdminMock;
            var claimHelper = ClaimHelperMocks.New();
            var logger = new DoNotLog();
            var expressionValidationService = new ExpressionValidationService(unitOfWork.Object, logger);
            var treatmentLoader = new ExcelTreatmentLoader(expressionValidationService);
            var treatmentService = new TreatmentService(unitOfWork.Object, treatmentLoader);
            pagingService ??= TreatmentPagingServiceMocks.EmptyMock;
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new TreatmentController(
                treatmentService,
                pagingService.Object,
                esecSecurity.Object,
                unitOfWork.Object,
                hubService.Object,
                contextAccessor.Object,
                claimHelper.Object,
                generalWorkQueue.Object
                );
            return controller;
        }
    }
}
