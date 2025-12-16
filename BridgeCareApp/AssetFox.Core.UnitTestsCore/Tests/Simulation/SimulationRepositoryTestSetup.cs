using AssetFox.Core.UnitTestsCore.Tests.Attributes.CalculatedAttributes;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class SimulationRepositoryTestSetup
    {
        public static void Setup()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            CalculatedAttributeTestSetup.CreateDefaultCalculatedAttributeLibrary(TestHelper.UnitOfWork);
        }

    }
}
