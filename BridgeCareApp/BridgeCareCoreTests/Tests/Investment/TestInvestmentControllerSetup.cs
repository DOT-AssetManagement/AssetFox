using System.Security.Claims;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Interfaces.DefaultData;
using AssetFoxCore.Utils;
using AssetFoxCoreTests.Tests.General_Work_Queue;
using AssetFoxCoreTests.Tests.SecurityUtilsClasses;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AssetFoxCoreTests.Tests
{
    public static class TestInvestmentControllerSetup
    {

        public static InvestmentController CreateController(
            Mock<IUnitOfWork> unitOfWork,
            IHttpContextAccessor contextAccessor,
            Mock<IHubService> hubServiceMock = null,
            Mock<IInvestmentBudgetsService> investmentBudgetServiceMock = null,
            Mock<IInvestmentPagingService> investmentPagingServiceMock = null
            )
        {
            var resolveHubService = hubServiceMock ?? HubServiceMocks.New();
            var security = EsecSecurityMocks.Dbe;
            var mockDataService = new Mock<IInvestmentDefaultDataService>();
            var simulationQueueService = new Mock<IWorkQueueService>();
            var claimHelper = new ClaimHelper(unitOfWork.Object, contextAccessor);
            var resolveInvestmentBudgetServiceMock = investmentBudgetServiceMock ?? InvestmentBudgetServiceMocks.New();
            var resolveInvestmentPagingServiceMock = investmentPagingServiceMock ?? new Mock<IInvestmentPagingService>();
            var generalWorkQueue = GeneralWorkQueueServiceMocks.New();
            var controller = new InvestmentController(
                resolveInvestmentBudgetServiceMock.Object,
                resolveInvestmentPagingServiceMock.Object,
                security,
                unitOfWork.Object,
                resolveHubService.Object,
                contextAccessor,
                mockDataService.Object,
                claimHelper,
                generalWorkQueue.Object);
            return controller;

        }

        public static InvestmentController CreateController(
            Mock<IUnitOfWork> unitOfWork,
            List<Claim> contextAccessorClaims,
            Mock<IHubService> hubServiceMock = null,
            Mock<IInvestmentBudgetsService> investmentBudgetServiceMock = null
            )
        {
            var accessorMock = HttpContextAccessorMocks.MockWithClaims(contextAccessorClaims);
            return CreateController(unitOfWork, accessorMock.Object, hubServiceMock, investmentBudgetServiceMock);
        }

        public static InvestmentController CreateAdminController(
            Mock<IUnitOfWork> unitOfWork,
            Mock<IHubService> hubServiceMock = null,
            Mock<IInvestmentBudgetsService> investmentBudgetServiceMock = null)
        {
            var claims = SystemSecurityClaimLists.Admin();
            var controller = CreateController(unitOfWork, claims, hubServiceMock, investmentBudgetServiceMock);
            return controller;
        }

        public static InvestmentController CreateNonAdminController(
            Mock<IUnitOfWork> unitOfWork,
            Mock<IHubService> hubServiceMock = null,
            Mock<IInvestmentBudgetsService> investmentBudgetServiceMock = null)
        {
            var claims = SystemSecurityClaimLists.Empty();
            var controller = CreateController(unitOfWork, claims, hubServiceMock, investmentBudgetServiceMock);
            return controller;
        }
    }
}
