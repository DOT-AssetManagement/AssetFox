using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore;

namespace AssetFox.Core.UnitTestsCore.TestUtils
{
    public static class TestAnalysisInputLoading
    {
        public static Simulation GetSimulationInput(Guid networkId, Guid simulationId)
        {
            Func<bool> returnTrue = () => true;
            var input = AnalysisInputLoading.GetSimulationWithAssets(TestHelper.UnitOfWork, networkId, simulationId,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue,
                returnTrue);
            return input;
        }
    }
}
