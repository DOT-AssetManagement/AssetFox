using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class ScenarioTreatmentConsequenceTestSetup
    {
        public static TreatmentConsequenceDTO ModelForEntityInDb(
            IUnitOfWork unitOfWork,
            Guid simulationId,
            Guid treatmentId,
            Guid? id = null,
            string attribute = "attribute",
            string equation = null,
            string criterion = null
            )
        {
            var consequence = TreatmentConsequenceDtos.Dto(id, attribute, equation, criterion);
            var costList = new List<TreatmentConsequenceDTO> { consequence };
            var dictionary = new Dictionary<Guid, List<TreatmentConsequenceDTO>> { { treatmentId, costList } };
            unitOfWork.TreatmentConsequenceRepo.UpsertOrDeleteScenarioTreatmentConsequences(dictionary, simulationId);
            return consequence;
        }
    }
}
