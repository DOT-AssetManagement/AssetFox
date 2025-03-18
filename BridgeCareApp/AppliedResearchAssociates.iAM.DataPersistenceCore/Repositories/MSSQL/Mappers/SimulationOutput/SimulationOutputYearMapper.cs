using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SimulationYearDetailMapper
    {
        public static SimulationYearDetailEntity ToEntityWithoutAssets(
            SimulationYearDetail domain,
            Guid simulationOutputId,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var id = Guid.NewGuid();
            var budgets = BudgetDetailMapper.ToEntityList(domain.Budgets, id);
            var deficientConditionGoals = DeficientConditionGoalDetailMapper.ToEntityList(domain.DeficientConditionGoals, id, attributeIdLookup);
            var targetConditionGoals = TargetConditionGoalDetailMapper.ToEntityList(domain.TargetConditionGoals, id, attributeIdLookup);
            var entity = new SimulationYearDetailEntity
            {
                Id = id,
                Budgets = budgets,
                ConditionOfNetwork = domain.ConditionOfNetwork,
                DeficientConditionGoals = deficientConditionGoals,
                SimulationOutputId = simulationOutputId,
                TargetConditionGoals = targetConditionGoals,
                Year = domain.Year,
            };
            return entity;
        }

        public static SimulationYearDetail ToDomainWithoutAssets(
            SimulationYearDetailEntity entity,
            Dictionary<Guid, string> attributeNameLookup
            )
        {
            var domain = new SimulationYearDetail(entity.Year)
            {
                ConditionOfNetwork = entity.ConditionOfNetwork,
            };
            var budgets = BudgetDetailMapper.ToDomainList(entity.Budgets);
            domain.Budgets.AddRange(budgets);
            var deficientConditionGoals = DeficientConditionGoalDetailMapper.ToDomainList(entity.DeficientConditionGoals, attributeNameLookup);
            domain.DeficientConditionGoals.AddRange(deficientConditionGoals);
            var targetConditionGoals = TargetConditionGoalDetailMapper.ToDomainList(entity.TargetConditionGoals, attributeNameLookup);
            domain.TargetConditionGoals.AddRange(targetConditionGoals);
            return domain;
        }

        public static List<SimulationYearDetail> ToDomainListWithoutAssets(
            ICollection<SimulationYearDetailEntity> entityList,
            Dictionary<Guid, string> attributeNameLookup)
        {
            var domainList = new List<SimulationYearDetail>();
            foreach (var entity in entityList)
            {
                var domain = ToDomainWithoutAssets(entity, attributeNameLookup);
                domainList.Add(domain);
            }
            return domainList;
        }
    }
}
