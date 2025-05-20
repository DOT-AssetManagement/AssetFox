using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BudgetDetailMapper
    {
        public static BudgetDetailEntity ToEntity(BudgetDetail budget, Guid simulationYearDetailId, int runId)
        {
            var entity = new BudgetDetailEntity
            {
                Id = SequentialGuid.NewGuid(),
                RunId = runId,
                SimulationYearDetailId = simulationYearDetailId,
                BudgetName = budget.BudgetName,
                AvailableFunding = budget.AvailableFunding,
            };
            return entity;
        }

        public static List<BudgetDetailEntity> ToEntityList(List<BudgetDetail> domainList, Guid id, int runId)
        {
            var entities = new List<BudgetDetailEntity>();
            foreach (var budget in domainList)
            {
                var mapBudget = ToEntity(budget, id, runId);
                entities.Add(mapBudget);
            }
            return entities;
        }

        public static BudgetDetail ToDomain(BudgetDetailEntity entity)
        {
            var domain = new BudgetDetail(entity.AvailableFunding, entity.BudgetName);
            return domain;
        }

        public static List<BudgetDetail> ToDomainList(ICollection<BudgetDetailEntity> entityList)
        {
            var domainList = new List<BudgetDetail>();
            foreach (var entity in entityList)
            {
                var domain = ToDomain(entity);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static BudgetDetailDTO ToDto(this BudgetDetailEntity entity)
        {
            var dto = new BudgetDetailDTO
            {
                Id = entity.Id,
                AvailableFunding = entity.AvailableFunding,
                BudgetName = entity.BudgetName
            };

            return dto;
        }

        public static BudgetDetailEntity ToEntity(this BudgetDetailDTO budgetDetailDto, Guid simulationYearDetailId)
        {
            return new BudgetDetailEntity
            {
                Id = budgetDetailDto.Id,
                SimulationYearDetailId = simulationYearDetailId,
                AvailableFunding = budgetDetailDto.AvailableFunding,
                BudgetName = budgetDetailDto.BudgetName
            };
        }
    }
}
