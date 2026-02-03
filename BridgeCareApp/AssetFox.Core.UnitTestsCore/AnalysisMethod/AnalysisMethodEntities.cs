using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.UnitTestsCore
{
    public static class AnalysisMethodEntities
    {
        public static AnalysisMethodEntity TestAnalysis(Guid simulationId, Guid? attributeId, Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var returnValue = new AnalysisMethodEntity
            {
                AttributeId = attributeId,
                Id = resolveId,
                SimulationId = simulationId,
                OptimizationStrategy = OptimizationStrategy.Benefit,
                SpendingStrategy = SpendingStrategy.NoSpending,
                ShouldApplyMultipleFeasibleCosts = false,
                ShouldDeteriorateDuringCashFlow = false,
                ShouldUseExtraFundsAcrossBudgets = false,
                shouldAllowMultipleTreatments = false
            };
            return returnValue;
        }
    }
}
