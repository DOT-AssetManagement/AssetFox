using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class CashFlowConsiderationDetailMapper
    {
        public static CashFlowConsiderationDetailEntity ToEntity(
            CashFlowConsiderationDetail domain,
            Guid treatmentConsiderationDetailId)
        {
            Guid id = Guid.NewGuid();
            var entity = new CashFlowConsiderationDetailEntity
            {
                Id = id,
                TreatmentConsiderationDetailId = treatmentConsiderationDetailId,
                CashFlowRuleName = domain.CashFlowRuleName,
                ReasonAgainstCashFlow = (int)domain.ReasonAgainstCashFlow,
            };
            return entity;
        }

        public static List<CashFlowConsiderationDetailEntity> ToEntityList(
            List<CashFlowConsiderationDetail> domainList,
            Guid treatmentConsiderationDetailId)
        {
            var entityList = new List<CashFlowConsiderationDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntity(domain, treatmentConsiderationDetailId);
                entityList.Add(entity);
            }
            return entityList;
        }

        public static CashFlowConsiderationDetail ToDomain(CashFlowConsiderationDetailEntity entity)
        {
            var domain = new CashFlowConsiderationDetail(entity.CashFlowRuleName)
            {
                ReasonAgainstCashFlow = (ReasonAgainstCashFlow)entity.ReasonAgainstCashFlow,
            };
            return domain;
        }

        public static List<CashFlowConsiderationDetail> ToDomainList(ICollection<CashFlowConsiderationDetailEntity> entityCollection)
        {
            var domainList = new List<CashFlowConsiderationDetail>();
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity);
                domainList.Add(domain);
            }
            return domainList;
        }

        public static CashFlowConsiderationDetailDTO ToDto(this CashFlowConsiderationDetailEntity entity)
        {
            var dto = new CashFlowConsiderationDetailDTO
            {
                Id = entity.Id,
                CashFlowRuleName = entity.CashFlowRuleName,
                ReasonAgainstCashFlow = entity.ReasonAgainstCashFlow    
            };

            return dto;
        }

        public static CashFlowConsiderationDetailEntity ToEntity(this CashFlowConsiderationDetailDTO cashFlowConsiderationDetailDto, Guid treatmentConsiderationDetailId)
        {
            return new CashFlowConsiderationDetailEntity
            {
                Id = cashFlowConsiderationDetailDto.Id,
                TreatmentConsiderationDetailId = treatmentConsiderationDetailId,
                ReasonAgainstCashFlow = cashFlowConsiderationDetailDto.ReasonAgainstCashFlow,
                CashFlowRuleName = cashFlowConsiderationDetailDto.CashFlowRuleName
            };
        }
    }
}
