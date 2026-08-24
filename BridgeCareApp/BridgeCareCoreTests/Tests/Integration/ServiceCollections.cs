using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers;
using AssetFoxCore.StartupExtension;
using AssetFoxCore.Utils;
using AssetFoxCore.Utils.Interfaces;
using AssetFoxCoreTests.Tests.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCoreTests.Tests
{
    public static class ServiceCollections
    {
        private static void AddTestUnitOfWork(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(TestHelper.UnitOfWork);
            serviceCollection.AddSingleton<IUnitOfWork>(x => x.GetRequiredService<UnitOfDataPersistenceWork>());
        }

        public static void AddReports(IServiceCollection serviceCollection)
        {
            var reportGenerator = ReportGeneratorMocks.New();
            var reportLookupFactory = ReportLookupFactoryMocks.Default();
            serviceCollection.AddSingleton(reportGenerator.Object);
            serviceCollection.AddSingleton(reportLookupFactory.Object);
        }

        public static ServiceCollection AdminControllersWithRequestForm(IFormCollection requestFormCollection)
        {
            var serviceCollection = Default();
            var security = EsecSecurityMocks.Admin;
            var contextAccessor = HttpContextAccessorMocks.AdminWithFormCollection(requestFormCollection);
            serviceCollection.AddSingleton(security);
            serviceCollection.AddSingleton(contextAccessor.Object);
            ManuallyAddControllers(serviceCollection);
            return serviceCollection;
        }

        public static ServiceCollection Default()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSimulationData();
            serviceCollection.AddPagingData();
            serviceCollection.AddDefaultData();
            serviceCollection.AddTestUnitOfWork();
            var hubService = HubServiceMocks.Default();
            serviceCollection.AddSingleton(hubService);
            serviceCollection.AddSignalR();
            AddReports(serviceCollection);
            serviceCollection.AddScoped<IClaimHelper, ClaimHelper>();
            var doNotLog = new DoNotLog();
            serviceCollection.AddSingleton<ILog>(doNotLog);
            return serviceCollection;
        }

        private static void ManuallyAddControllers(ServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AdminDataController>();
            serviceCollection.AddScoped<NetworkController>();
            serviceCollection.AddScoped<AdminSiteSettingsController>();
            serviceCollection.AddScoped<AggregationController>();
            serviceCollection.AddScoped<AnalysisMethodController>();
            serviceCollection.AddScoped<AnnouncementController>();
            serviceCollection.AddScoped<AttributeController>();
            serviceCollection.AddScoped<BenefitQuantifierController>();
            serviceCollection.AddScoped<BudgetPriorityController>();
            serviceCollection.AddScoped<CalculatedAttributesController>();
            serviceCollection.AddScoped<CashFlowController>();
            serviceCollection.AddScoped<CommittedProjectController>();
            serviceCollection.AddScoped<CriterionLibraryController>();
            serviceCollection.AddScoped<DataSourceController>();
            serviceCollection.AddScoped<DeficientConditionGoalController>();
            serviceCollection.AddScoped<ExpressionValidationController>();
            serviceCollection.AddScoped<InventoryController>();
            serviceCollection.AddScoped<NetworkController>();
            serviceCollection.AddScoped<PerformanceCurveController>();
            serviceCollection.AddScoped<RawDataController>();
            serviceCollection.AddScoped<RemainingLifeLimitController>();
            serviceCollection.AddScoped<SimulationController>();
            serviceCollection.AddScoped<SimulationLogController>();
            serviceCollection.AddScoped<TargetConditionGoalController>();
            serviceCollection.AddScoped<TreatmentController>();
            serviceCollection.AddScoped<UserController>();
            serviceCollection.AddScoped<UserCriteriaController>();
        }

        internal static IServiceCollection DbeControllersForUser(UserDTO user)
        {
            var serviceCollection = Default();
            var security = EsecSecurityMocks.DbeForUser(user);
            var contextAccessor = HttpContextAccessorMocks.Default();
            serviceCollection.AddSingleton(security);
            serviceCollection.AddSingleton(contextAccessor);
            ManuallyAddControllers(serviceCollection);
            return serviceCollection;
        }
    }
}
