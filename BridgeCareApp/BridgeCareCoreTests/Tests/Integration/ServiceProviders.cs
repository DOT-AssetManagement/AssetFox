using AppliedResearchAssociates.iAM.DTOs;
using BridgeCareCoreTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BridgeCareCoreTests.Tests.Integration
{
    public static class ServiceProviders
    {
        private static IServiceProvider _default { get; set; }
        private static IServiceProvider _adminControllers { get; set; }

        private static IServiceProvider _dbeControllers { get; set; }

        private static IServiceProvider CreateDefault()
        {
            var serviceCollection = ServiceCollections.Default();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static IServiceProvider CreateAdminControllers()
        {
            var serviceCollection = ServiceCollections.AdminControllers();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private static IServiceProvider CreateDbeControllers()
        {
            var serviceCollection = ServiceCollections.DbeControllers();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        /// <summary>Caches the serviceProvider. Hence it is created only
        /// on the first call.</summary> 
        public static IServiceProvider AdminControllers()
        {
            if (_adminControllers == null)
            {
                _adminControllers = CreateAdminControllers();
            }
            return _adminControllers;
        }

        /// <summary>Builds a separate serviceProvider for every call.
        /// If you don't have any formFiles, consider using a caching call instead,
        /// such as AdminControllers().</summary> 
        public static IServiceProvider AdminControllersWithFiles(params IFormFile[] files)
        {
            var serviceCollection = ServiceCollections.AdminControllersWithFormFiles(files);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

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

        public static IServiceProvider AdminControllersWithLibraryIdAndFiles(Guid libraryId, params IFormFile[] files)
        {
            var formCollection = FormCollectionMocks.FormWithLibraryIdAndFiles(libraryId, files);
            var serviceProvider = AdminControllersWithRequestForm(formCollection);
            return serviceProvider;
        }

        public static IServiceProvider DbeControllers()
        {
            if (_dbeControllers == null)
            {
                _dbeControllers = CreateDbeControllers();
            }
            return _dbeControllers;
        }

        public static IServiceProvider Default()
        {
            if (_default == null)
            {
                _default = CreateDefault();
            }
            return _default;
        }

        internal static IServiceProvider DbeControllersForUser(UserDTO user)
        {
            var serviceCollection = ServiceCollections.DbeControllersForUser(user);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
