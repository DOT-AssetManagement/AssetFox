using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class CriterionLibraryBudgetJoinMapper
    {
        public static CriterionLibraryBudgetEntity ToEntity(
            this CriterionLibraryBudgetDTO dto)
        {
            var entity = new CriterionLibraryBudgetEntity
            {
                CriterionLibraryId = dto.CriterionLibraryId,
                BudgetId = dto.BudgetId,
            };
            return entity;
        }
    }
}
