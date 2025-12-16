using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;

namespace AssetFox.Core.UnitTestsCore.Tests
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
