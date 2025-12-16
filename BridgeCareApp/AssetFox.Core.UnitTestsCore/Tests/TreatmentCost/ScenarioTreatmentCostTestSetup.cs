using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace AssetFox.Core.UnitTestsCore.Tests.TreatmentCost
{
    public static class ScenarioTreatmentCostTestSetup
    {
        public static TreatmentCostDTO CostForTreatmentInDb(
            IUnitOfWork unitOfWork,
            Guid treatmentId,
            Guid simulationId,
            Guid? id = null,
            string mergedCriteriaExpression = null,
            string equation = "[AGE]")
        {
            var cost = TreatmentCostDtos.WithEquationAndCriterionLibrary(id, equation:equation, mergedCriteriaExpression: mergedCriteriaExpression);
            var costs = new List<TreatmentCostDTO> { cost };
            var costDictionary = new Dictionary<Guid, List<TreatmentCostDTO>>
            {
                {
                treatmentId,
                costs
                }
            };
            unitOfWork.TreatmentCostRepo.UpsertOrDeleteScenarioTreatmentCosts(
                costDictionary, simulationId);
            return cost;
        }
    }
}
