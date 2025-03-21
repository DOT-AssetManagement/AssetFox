using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DataPersistenceCore;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
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
