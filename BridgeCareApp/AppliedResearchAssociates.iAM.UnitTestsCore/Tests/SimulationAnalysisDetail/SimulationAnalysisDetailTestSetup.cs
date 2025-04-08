using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class SimulationAnalysisDetailTestSetup
    {
        public static void CreateAnalysisDetail(UnitOfDataPersistenceWork unitOfWork, Guid simulationId, string status = "Completed")
        {
            var analysisDetail = SimulationAnalysisDetailDtos.ForSimulation(simulationId, status);
            unitOfWork.SimulationAnalysisDetailRepo.UpsertSimulationAnalysisDetail(analysisDetail);
        }
    }
}
