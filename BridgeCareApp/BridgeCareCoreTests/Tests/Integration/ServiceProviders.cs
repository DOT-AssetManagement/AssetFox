using AppliedResearchAssociates.iAM.DTOs;
using BridgeCareCoreTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeCareCoreTests.Tests.Integration
{
    // WJPRQ from the rejected branch
    public static class ServiceProviders
    {
        private static IServiceProvider _adminControllers { get; set; }

        private static IServiceProvider AdminControllersWithRequestForm(IFormCollection requestFormCollection)
        {
            var serviceCollection = ServiceCollections.AdminControllersWithRequestForm(requestFormCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        public static IServiceProvider AdminControllersWithSimulationIdAndFiles(Guid simulationId, params IFormFile[] files)
        {
            var formCollection = FormCollectionMocks.FormWithSimulationIdAndFiles(simulationId, files);
            var serviceProvider = AdminControllersWithRequestForm(formCollection);
            return serviceProvider;
        }
    }
}
