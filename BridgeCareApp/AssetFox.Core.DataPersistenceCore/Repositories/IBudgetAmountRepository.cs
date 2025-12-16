using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Models;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IBudgetAmountRepository
    {
        void UpsertOrDeleteBudgetAmounts(Dictionary<Guid, List<BudgetAmountDTO>> budgetAmountsPerBudgetId, Guid libraryId);

        void UpsertOrDeleteScenarioBudgetAmounts(Dictionary<Guid, List<BudgetAmountDTO>> budgetAmountsPerBudgetId,
            Guid simulationId);

        List<BudgetAmountDTO> GetLibraryBudgetAmounts(Guid libraryId);

        List<BudgetAmountDTO> GetScenarioBudgetAmounts(Guid simulationId);
        void SaveScenarioBudgetAmounts(InvestmentUpsertAndDeleteModel changes, Guid simulationId);
        void SaveLibraryBudgetAmounts(InvestmentUpsertAndDeleteModel changes, Guid simulationId);
    }
}
