using AssetFox.Core.DTOs;
using AssetFoxCoreTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AssetFoxCoreTests.Tests.Integration
{
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
