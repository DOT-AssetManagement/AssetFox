using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Analysis.Engine;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class SimulationYearDetailMapper
    {
        public static SimulationYearDetailEntity ToEntityWithoutAssets(
            SimulationYearDetail domain,
            Guid simulationOutputId,
            Dictionary<string, Guid> attributeIdLookup,
            int simulationRunId)
        {
            var id = SequentialGuid.NewGuid();
            var budgets = BudgetDetailMapper.ToEntityList(domain.Budgets, id, simulationRunId);
            var deficientConditionGoals = DeficientConditionGoalDetailMapper.ToEntityList(domain.DeficientConditionGoals, id, attributeIdLookup, simulationRunId);
            var targetConditionGoals = TargetConditionGoalDetailMapper.ToEntityList(domain.TargetConditionGoals, id, attributeIdLookup, simulationRunId);
            var entity = new SimulationYearDetailEntity
            {
                Id = id,
                Budgets = budgets,
                ConditionOfNetwork = domain.ConditionOfNetwork,
                DeficientConditionGoals = deficientConditionGoals,
                SimulationOutputId = simulationOutputId,
                TargetConditionGoals = targetConditionGoals,
                RunId = simulationRunId,
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

        public static SimulationYearDetailDTO ToDto(this SimulationYearDetailEntity entity)
        {
            var dto = new SimulationYearDetailDTO
            {
                Id = entity.Id,
                Year = entity.Year,
                ConditionOfNetwork = entity.ConditionOfNetwork,
                Budgets = entity.Budgets.Select(_ => _.ToDto()).ToList(),
                DeficientConditionGoals = entity.DeficientConditionGoals.Select(_ => _.ToDto()).ToList(),
                TargetConditionGoals = entity.TargetConditionGoals.Select(_ => _.ToDto()).ToList(),
                Assets = entity.Assets.Select(_ => _.ToDto()).ToList()
            };

            return dto;
        }

        public static SimulationYearDetailEntity ToEntity(this SimulationYearDetailDTO simulationYearDetailDto, Guid simulationOutputId)
        {
            var simulationYearDetailId = simulationYearDetailDto.Id;

            return new SimulationYearDetailEntity
            {
                Id = simulationYearDetailDto.Id,
                SimulationOutputId = simulationOutputId,
                Year = simulationYearDetailDto.Year,
                ConditionOfNetwork = simulationYearDetailDto.ConditionOfNetwork,
                Assets = simulationYearDetailDto.Assets.Select(_ => _.ToEntity(simulationYearDetailId)).ToList(),
                Budgets = simulationYearDetailDto.Budgets.Select(_ => _.ToEntity(simulationYearDetailId)).ToList(),
                DeficientConditionGoals = simulationYearDetailDto.DeficientConditionGoals.Select(_ => _.ToEntity(simulationYearDetailId)).ToList(),
                TargetConditionGoals = simulationYearDetailDto.TargetConditionGoals.Select(_ => _.ToEntity(simulationYearDetailId)).ToList()
            };
        }
    }
}
