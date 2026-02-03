using System;
using System.Collections.Generic;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class ScenarioBudgetTestSetup
    {
        public static void UpsertOrDeleteScenarioBudgets(
            IUnitOfWork unitOfWork,
            List<BudgetDTO> budgets,
            Guid simulationId
            )
        {
            unitOfWork.BudgetRepo.UpsertOrDeleteScenarioBudgets(
                budgets,
                simulationId);
        }
    }
}
