using System;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using System.Collections.Generic;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class BenefitMapper
    {
        public static BenefitEntity ToEntity(this Benefit domain, Guid analysisMethodId, Guid attributeId) =>
            new BenefitEntity
            {
                Id = domain.Id,
                AnalysisMethodId = analysisMethodId,
                Limit = domain.Limit,
                AttributeId = attributeId
            };

        public static BenefitEntity ToEntity(this BenefitDTO dto, Guid analysisMethodId, Guid attributeId, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new BenefitEntity

            {
                Id = dto.Id,
                AnalysisMethodId = analysisMethodId,
                Limit = dto.Limit,
                AttributeId = attributeId
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }

        public static BenefitDTO ToDto(this BenefitEntity entity, IReadOnlyDictionary<Guid, string> attributeNameLookup)
        {
            var attributeName = attributeNameLookup[entity.AttributeId];
            return new BenefitDTO { Id = entity.Id, Limit = entity.Limit, Attribute = attributeName };
        }
    }
}
