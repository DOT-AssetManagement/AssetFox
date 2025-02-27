using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;

namespace BridgeCareCoreTests.Tests.Integration
{
    public static class FormCollectionMocks
    {

        public static IFormCollection FormWithFiles(IFormFile[] files)
        {
            var mock = new Mock<IFormCollection>();
            var formFileCollection = new FormFileCollection();
            formFileCollection.AddRange(files);
            mock.Setup(m => m.Files).Returns(formFileCollection);
            return mock.Object;
        }

        public static IFormCollection FormWithSimulationIdAndFiles(Guid simulationId, params IFormFile[] files)
        {
            var mock = FormWithIdAndFiles("simulationId", simulationId, files);
            return mock;
        }

        public static IFormCollection FormWithLibraryIdAndFiles(Guid libraryId, params IFormFile[] files)
        {
            var mock = FormWithIdAndFiles("libraryId", libraryId, files);
            return mock;
        }

        /// <summary>Puts the passed-in files into the a form collection. Sets up
        /// the mock to return the form collection. Also sets it up to return the given id
        /// for the requested key when TryGetValue is called.</summary> 
        public static IFormCollection FormWithIdAndFiles(string keyForId, Guid id, params IFormFile[] files)
        {
            var mock = new Mock<IFormCollection>();
            var formFileCollection = new FormFileCollection();
            formFileCollection.AddRange(files);
            mock.Setup(m => m.Files).Returns(formFileCollection);
            var simulationIdString = id.ToString();
            mock.SetupGetValue(keyForId, simulationIdString);
            return mock.Object;
        }
    }
}
