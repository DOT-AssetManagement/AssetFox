using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class TreatmentOptionDetailMapper
    {
        public static TreatmentOptionDetailEntity ToEntity(
            TreatmentOptionDetail domain,
            Guid assetDetailId,
            int runId)
        {
            var id = Guid.NewGuid();
            var entity = new TreatmentOptionDetailEntity
            {
                Id = id,
                AssetDetailId = assetDetailId,
                Benefit = domain.Benefit,
                Cost = domain.Cost,
                RemainingLife = domain.RemainingLife,
                TreatmentName = domain.TreatmentName,
                ConditionChange = domain.ConditionChange,
                RunId = runId
            };
            return entity;
        }

        public static List<TreatmentOptionDetailEntity> ToEntityList(
            List<TreatmentOptionDetail> domainList,
            Guid assetDetailId,
            int runId
            )
        {
            var list = new List<TreatmentOptionDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntity(domain, assetDetailId, runId);
                list.Add(entity);
            }
            return list;
        }

        public static TreatmentOptionDetail ToDomain(TreatmentOptionDetailEntity entity)
        {
            var domain = new TreatmentOptionDetail(entity.TreatmentName, entity.Cost, entity.Benefit, entity.RemainingLife, entity.ConditionChange);
            return domain;
        }

        internal static List<TreatmentOptionDetail> ToDomainList(ICollection<TreatmentOptionDetailEntity> entityCollection)
        {
            var domainList = new List<TreatmentOptionDetail>();
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static TreatmentOptionDetailDTO ToDto(this TreatmentOptionDetailEntity entity)
        {
            var dto = new TreatmentOptionDetailDTO
            {
                Id = entity.Id,
                Benefit = entity.Benefit,
                RemainingLife = entity.RemainingLife,
                ConditionChange = entity.ConditionChange,
                TreatmentName = entity.TreatmentName,
                Cost = entity.Cost
            };

            return dto;
        }

        public static TreatmentOptionDetailEntity ToEntity(this TreatmentOptionDetailDTO treatmentOptionDetailDto, Guid assetDetailId)
        {
            return new TreatmentOptionDetailEntity
            {
                Id = treatmentOptionDetailDto.Id,
                AssetDetailId = assetDetailId,
                TreatmentName = treatmentOptionDetailDto.TreatmentName,
                Benefit = treatmentOptionDetailDto.Benefit,
                ConditionChange = treatmentOptionDetailDto.ConditionChange,
                Cost = treatmentOptionDetailDto.Cost,
                RemainingLife = treatmentOptionDetailDto.RemainingLife
            };
        }
    }
}
