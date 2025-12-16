using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class CriterionLibraryScenarioBudgetJoinMapper
    {
        public static CriterionLibraryScenarioBudgetEntity ToEntity(
            this CriterionLibraryScenarioBudgetDTO dto)
        {
            var entity = new CriterionLibraryScenarioBudgetEntity
            {
                CriterionLibraryId = dto.CriterionLibraryId,
                ScenarioBudgetId = dto.ScenarioBudgetId,
            };
            return entity;
        }
    }
}
