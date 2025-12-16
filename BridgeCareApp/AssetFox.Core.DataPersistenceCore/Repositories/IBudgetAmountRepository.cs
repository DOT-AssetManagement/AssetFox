using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AppliedResearchAssociates.iAM.Analysis;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Models;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
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
