using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class TargetConditionGoalDetailMapper
    {
        public static TargetConditionGoalDetailEntity ToEntity(
            this TargetConditionGoalDetail domain,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup,
            int runId)
        {
            var id = Guid.NewGuid();
            var attributeId = attributeIdLookup[domain.AttributeName];
            var entity = new TargetConditionGoalDetailEntity
            {
                Id = id,
                RunId = runId,
                ActualValue = domain.ActualValue,
                AttributeId = attributeId,
                GoalIsMet = domain.GoalIsMet,
                GoalName = domain.GoalName,
                SimulationYearDetailId = simulationYearDetailId,
                TargetValue = domain.TargetValue,
            };
            return entity;
        }

        public static List<TargetConditionGoalDetailEntity> ToEntityList(
            List<TargetConditionGoalDetail> domainList,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup,
            int runId)
        {
            var entities = new List<TargetConditionGoalDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntity(domain, simulationYearDetailId, attributeIdLookup, runId);
                entities.Add(entity);
            }
            return entities;
        }

        public static TargetConditionGoalDetail ToDomain(TargetConditionGoalDetailEntity entity, Dictionary<Guid, string> attributeNameLookup)
        {
            var attributeName = attributeNameLookup[entity.AttributeId];
            var domain = new TargetConditionGoalDetail
            {
                ActualValue = entity.ActualValue,
                AttributeName = attributeName,
                GoalIsMet = entity.GoalIsMet,
                GoalName = entity.GoalName,
                TargetValue = entity.TargetValue,
            };
            return domain;
        }

        public static List<TargetConditionGoalDetail> ToDomainList(ICollection<TargetConditionGoalDetailEntity> entityCollection, Dictionary<Guid, string> attributeNameLookup)
        {
            var domainList = new List<TargetConditionGoalDetail>();
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity, attributeNameLookup);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static TargetConditionGoalDetailDTO ToDto(this TargetConditionGoalDetailEntity entity)
        {
            var dto = new TargetConditionGoalDetailDTO
            {
                Id = entity.Id,
                ActualValue = entity.ActualValue,
                TargetValue = entity.TargetValue,
                AttributeId = entity.AttributeId,
                GoalIsMet = entity.GoalIsMet,
                GoalName = entity.GoalName
            };

            return dto;
        }

        public static TargetConditionGoalDetailEntity ToEntity(this TargetConditionGoalDetailDTO targetConditionGoalDetailDto, Guid simulationYearDetailId)
        {
            return new TargetConditionGoalDetailEntity
            {
                Id = targetConditionGoalDetailDto.Id,
                SimulationYearDetailId = simulationYearDetailId,
                ActualValue = targetConditionGoalDetailDto.ActualValue,
                TargetValue = targetConditionGoalDetailDto.TargetValue,
                AttributeId = targetConditionGoalDetailDto.AttributeId,
                GoalIsMet = targetConditionGoalDetailDto.GoalIsMet,
                GoalName = targetConditionGoalDetailDto.GoalName
            };
        }
    }
}
