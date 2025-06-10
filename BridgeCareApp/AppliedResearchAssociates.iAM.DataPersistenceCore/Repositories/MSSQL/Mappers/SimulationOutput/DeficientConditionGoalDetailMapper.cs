using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class DeficientConditionGoalDetailMapper
    {
        public static List<DeficientConditionGoalDetailEntity> ToEntityList(
            List<DeficientConditionGoalDetail> domainList,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup, int runId)
        {
            var entities = new List<DeficientConditionGoalDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntity(domain, simulationYearDetailId, attributeIdLookup, runId);
                entities.Add(entity);
            }
            return entities;
        }

        private static DeficientConditionGoalDetailEntity ToEntity(
            DeficientConditionGoalDetail domain,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup,
            int runId)
        {
            var attributeId = attributeIdLookup[domain.AttributeName];
            var id = SequentialGuid.NewGuid();
            var entity = new DeficientConditionGoalDetailEntity
            {
                Id = id,
                RunId = runId,
                ActualDeficientPercentage = domain.ActualDeficientPercentage,
                AllowedDeficientPercentage = domain.AllowedDeficientPercentage,
                AttributeId = attributeId,
                DeficientLimit = domain.DeficientLimit,
                GoalIsMet = domain.GoalIsMet,
                GoalName = domain.GoalName,
                SimulationYearDetailId = simulationYearDetailId,
            };
            return entity;
        }

        public static DeficientConditionGoalDetail ToDomain(
            DeficientConditionGoalDetailEntity entity,
            Dictionary<Guid, string> attributeNameLookup
            )
        {
            var attributeName = attributeNameLookup.GetAttributeNameOrEmptyString(entity.AttributeId);
            var domain = new DeficientConditionGoalDetail
            {
                ActualDeficientPercentage = entity.ActualDeficientPercentage,
                AllowedDeficientPercentage = entity.AllowedDeficientPercentage,
                AttributeName = attributeName,
                DeficientLimit = entity.DeficientLimit,
                GoalIsMet = entity.GoalIsMet,
                GoalName = entity.GoalName,
            };
            return domain;
        }

        public static List<DeficientConditionGoalDetail> ToDomainList(
            ICollection<DeficientConditionGoalDetailEntity> entityCollection,
            Dictionary<Guid, string> attributeNameLookup
            )
        {
            var domainList = new List<DeficientConditionGoalDetail>();
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity, attributeNameLookup);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static DeficientConditionGoalDetailDTO ToDto(this DeficientConditionGoalDetailEntity entity)
        {
            var dto = new DeficientConditionGoalDetailDTO
            {
                Id = entity.Id,
                ActualDeficientPercentage= entity.ActualDeficientPercentage,
                AllowedDeficientPercentage = entity.AllowedDeficientPercentage,
                DeficientLimit = entity.DeficientLimit,
                GoalIsMet = entity.GoalIsMet,
                GoalName = entity.GoalName,
                AttributeId = entity.AttributeId
            };

            return dto;
        }

        public static DeficientConditionGoalDetailEntity ToEntity(this DeficientConditionGoalDetailDTO deficientConditionGoalDetailDto, Guid simulationYearDetailId)
        {
            return new DeficientConditionGoalDetailEntity
            {
                Id = deficientConditionGoalDetailDto.Id,
                SimulationYearDetailId = simulationYearDetailId,
                ActualDeficientPercentage = deficientConditionGoalDetailDto.ActualDeficientPercentage,
                AllowedDeficientPercentage = deficientConditionGoalDetailDto.AllowedDeficientPercentage,                
                DeficientLimit = deficientConditionGoalDetailDto.DeficientLimit,
                AttributeId = deficientConditionGoalDetailDto.AttributeId,
                GoalIsMet = deficientConditionGoalDetailDto.GoalIsMet,
                GoalName = deficientConditionGoalDetailDto.GoalName
            };
        }
    }
}
