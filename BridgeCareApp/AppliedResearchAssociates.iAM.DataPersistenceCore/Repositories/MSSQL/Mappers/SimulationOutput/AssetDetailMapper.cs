using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Entities;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class AssetDetailMapper
    {
        public static AssetDetailEntity ToEntityWithoutChildEntities(
            AssetDetail domain,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var id = Guid.NewGuid();
            var entity = new AssetDetailEntity
            {
                Id = id,
                MaintainableAssetId = domain.AssetId,
                SimulationYearDetailId = simulationYearDetailId,
                AppliedTreatment = domain.AppliedTreatment,
                TreatmentCause = (int)domain.TreatmentCause,
                TreatmentFundingIgnoresSpendingLimit = domain.TreatmentFundingIgnoresSpendingLimit,
                TreatmentStatus = (int)domain.TreatmentStatus,
                ProjectSource = domain.ProjectSource
            };
            return entity;
        }

        public static List<AssetDetailEntity> ToEntityList(
            List<AssetDetail> domainList,
            Guid simulationYearDetailId,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var entities = new List<AssetDetailEntity>();
            foreach (var domain in domainList)
            {
                var entity = ToEntityWithoutChildEntities(domain, simulationYearDetailId, attributeIdLookup);
                entities.Add(entity);
            }
            return entities;
        }

        public static AssetDetail ToDomain(
            AssetDetailEntity entity,
            int year,
            Dictionary<Guid, string> attributeNameLookup,
            Dictionary<Guid, string> assetNameLookup)
        {
            var assetName = assetNameLookup[entity.MaintainableAssetId];
            var domain = new AssetDetail(assetName, entity.MaintainableAssetId)
            {
                AppliedTreatment = entity.AppliedTreatment,
                TreatmentCause = (TreatmentCause)entity.TreatmentCause,
                TreatmentFundingIgnoresSpendingLimit = entity.TreatmentFundingIgnoresSpendingLimit,
                TreatmentStatus = (TreatmentStatus)entity.TreatmentStatus,
                ProjectSource = entity.ProjectSource
            };
            AssetDetailValueMapper.AddToDictionaries(entity.AssetDetailValuesIntId, domain.ValuePerTextAttribute, domain.ValuePerNumericAttribute, attributeNameLookup);
            var treatmentConsiderations = TreatmentConsiderationDetailMapper.ToDomainList(entity.TreatmentConsiderations);
            domain.TreatmentConsiderations.AddRange(treatmentConsiderations);
            var treatmentOptions = TreatmentOptionDetailMapper.ToDomainList(entity.TreatmentOptions);
            domain.TreatmentOptions.AddRange(treatmentOptions);
            var treatmentRejections = TreatmentRejectionDetailMapper.ToDomainList(entity.TreatmentRejections);
            domain.TreatmentRejections.AddRange(treatmentRejections);
            var treatmentSchedulingCollisions = TreatmentSchedulingCollisionDetailMapper.ToDomainList(entity.TreatmentSchedulingCollisions, year);
            domain.TreatmentSchedulingCollisions.AddRange(treatmentSchedulingCollisions);
            return domain;
        }

        internal static void AppendToDomainDictionaryWithValues(
            Dictionary<Guid, AssetDetail> dictionary,
            ICollection<AssetDetailEntity> entityCollection,
            int year,
            Dictionary<Guid, string> attributeNameLookup,
            Dictionary<Guid, string> assetNameLookup)
        {
            foreach (var entity in entityCollection)
            {
                var domain = ToDomain(entity, year, attributeNameLookup, assetNameLookup);
                dictionary[entity.Id] = domain;
                foreach (var assetDetailValue in entity.AssetDetailValuesIntId)
                {
                    AssetDetailValueMapper.AddToDictionary(assetDetailValue, domain.ValuePerTextAttribute, domain.ValuePerNumericAttribute, attributeNameLookup);
                }
            }
        }

        internal static AssetDetailEntityFamily ToEntityFamily(
            List<AssetDetail> assets,
            Guid yearDetailId,
            Dictionary<string, Guid> attributeIdLookup)
        {
            var family = new AssetDetailEntityFamily();
            foreach (var asset in assets)
            {
                AddToFamily(family, asset, yearDetailId, attributeIdLookup);
            }
            return family;
        }

        private static void AddToFamily(
            AssetDetailEntityFamily family,
            AssetDetail domain,
            Guid yearDetailId,
            Dictionary<string, Guid> attributeIdLookup
            )
        {
            var entity = ToEntityWithoutChildEntities(domain, yearDetailId, attributeIdLookup);
            family.AssetDetails.Add(entity);
            var mapNumericValues = AssetDetailValueMapper.ToNumericEntityList(entity.Id, domain.ValuePerNumericAttribute, attributeIdLookup);
            var mapTextValues = AssetDetailValueMapper.ToTextEntityList(entity.Id, domain.ValuePerTextAttribute, attributeIdLookup);
            var treatmentOptions = TreatmentOptionDetailMapper.ToEntityList(domain.TreatmentOptions, entity.Id);
            var treatmentRejections = TreatmentRejectionDetailMapper.ToEntityList(domain.TreatmentRejections, entity.Id);
            TreatmentConsiderationDetailMapper.AddToFamily(entity.Id, family, domain.TreatmentConsiderations); 
            var treatmentSchedulingCollisions = TreatmentSchedulingCollisionDetailMapper.ToEntityList(domain.TreatmentSchedulingCollisions, entity.Id);

            family.AssetDetailValues.AddRange(mapNumericValues);
            family.AssetDetailValues.AddRange(mapTextValues);
            family.TreatmentOptions.AddRange(treatmentOptions);
            family.TreatmentRejections.AddRange(treatmentRejections);
            family.TreatmentSchedulingCollisions.AddRange(treatmentSchedulingCollisions);
        }

        public static AssetDetailDTO ToDto(this AssetDetailEntity entity)
        {
            var dto = new AssetDetailDTO
            {
                Id = entity.Id,
                SimulationYearDetailId = entity.SimulationYearDetailId,
                MaintainableAssetId = entity.MaintainableAssetId,
                AppliedTreatment = entity.AppliedTreatment,
                ProjectSource = entity.ProjectSource,
                TreatmentCause = entity.TreatmentCause,
                TreatmentFundingIgnoresSpendingLimit = entity.TreatmentFundingIgnoresSpendingLimit,
                TreatmentStatus = entity.TreatmentStatus,
                AssetDetailValuesIntId = entity.AssetDetailValuesIntId.Select(_ => _.ToDto()).ToList(),
                TreatmentConsiderations = entity.TreatmentConsiderations.Select(_ => _.ToDto()).ToList(),
                TreatmentOptions = entity.TreatmentOptions.Select(_ => _.ToDto()).ToList(),
                TreatmentRejections = entity.TreatmentRejections.Select(_ => _.ToDto()).ToList(),
                TreatmentSchedulingCollisions = entity.TreatmentSchedulingCollisions.Select(_ => _.ToDto()).ToList()
            };

            return dto;
        }
    }
}
