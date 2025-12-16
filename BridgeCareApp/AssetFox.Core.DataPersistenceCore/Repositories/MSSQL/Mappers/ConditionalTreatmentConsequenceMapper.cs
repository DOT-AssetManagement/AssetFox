using System;
using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs;
using Attribute = AssetFox.Core.Analysis.Attribute;
using AssetFox.Core.DTOs.Static;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Mappers
{
    public static class ConditionalTreatmentConsequenceMapper
    {
        public static ScenarioConditionalTreatmentConsequenceEntity ToScenarioEntity(this ConditionalTreatmentConsequence domain, Guid treatmentId, Guid attributeId) =>
            new ScenarioConditionalTreatmentConsequenceEntity
            {
                Id = domain.Id,
                ScenarioSelectableTreatmentId = treatmentId,
                AttributeId = attributeId,
                ChangeValue = domain.Change.Expression
            };

        public static ConditionalTreatmentConsequenceEntity ToLibraryEntity(this TreatmentConsequenceDTO dto, Guid treatmentId,
            Guid attributeId) =>
            new ConditionalTreatmentConsequenceEntity
            {
                Id = dto.Id,
                SelectableTreatmentId = treatmentId,
                AttributeId = attributeId,
                ChangeValue = dto.ChangeValue
            };

        public static ScenarioConditionalTreatmentConsequenceEntity ToScenarioEntity(this TreatmentConsequenceDTO dto, Guid treatmentId, Guid attributeId, BaseEntityProperties baseEntityProperties = null)
        {
            var entity = new ScenarioConditionalTreatmentConsequenceEntity
            {
                Id = dto.Id,
                ScenarioSelectableTreatmentId = treatmentId,
                AttributeId = attributeId,
                ChangeValue = dto.ChangeValue
            };
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }

        public static ScenarioConditionalTreatmentConsequenceEntity ToScenarioEntityWithCriterionLibraryJoin(this TreatmentConsequenceDTO dto, Guid treatmentId, Guid attributeId, BaseEntityProperties baseEntityProperties)
        {

            var entity = ToScenarioEntity(dto, treatmentId, attributeId, baseEntityProperties);
            var criterionLibraryDto = dto.CriterionLibrary;
            var isvalid = criterionLibraryDto.IsValid();
            if (isvalid)
            {
                var criterionLibrary = criterionLibraryDto.ToSingleUseEntity(baseEntityProperties);
                var join = new CriterionLibraryScenarioConditionalTreatmentConsequenceEntity
                {
                    ScenarioConditionalTreatmentConsequenceId = entity.Id,
                    CriterionLibrary = criterionLibrary,
                };
                BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
                BaseEntityPropertySetter.SetBaseEntityProperties(join, baseEntityProperties);
                entity.CriterionLibraryScenarioConditionalTreatmentConsequenceJoin = join;
            }
            if (dto.Equation != null && dto.Equation.Id != Guid.Empty)
            {
                var equationEntity = EquationMapper.ToEntity(dto.Equation, baseEntityProperties);
                var equationJoin = new ScenarioConditionalTreatmentConsequenceEquationEntity
                {
                    Equation = equationEntity,
                    ScenarioConditionalTreatmentConsequenceId = entity.Id,
                };
                BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
                BaseEntityPropertySetter.SetBaseEntityProperties(equationJoin, baseEntityProperties);
                entity.ScenarioConditionalTreatmentConsequenceEquationJoin = equationJoin;
            }
            BaseEntityPropertySetter.SetBaseEntityProperties(entity, baseEntityProperties);
            return entity;
        }


        public static void CreateConditionalTreatmentConsequence(this ScenarioConditionalTreatmentConsequenceEntity entity, SelectableTreatment treatment, IEnumerable<Attribute> attributes)
        {
            var consequence = treatment.AddConsequence();
            consequence.Id = entity.Id;
            consequence.Attribute = entity.Attribute.GetAttributesFromDomain(attributes);
            consequence.Change.Expression = entity.ChangeValue;
            consequence.Criterion.Expression = entity.CriterionLibraryScenarioConditionalTreatmentConsequenceJoin?.CriterionLibrary
                .MergedCriteriaExpression ?? string.Empty;
            consequence.Equation.Expression = entity.ScenarioConditionalTreatmentConsequenceEquationJoin?.Equation.Expression ?? string.Empty;
        }

        public static TreatmentConsequenceDTO ToDto(this ConditionalTreatmentConsequenceEntity entity) =>
            new TreatmentConsequenceDTO
            {
                Id = entity.Id,
                ChangeValue = entity.ChangeValue,
                Attribute = entity.Attribute != null
                    ? entity.Attribute.Name
                    : "",
                Equation = entity.ConditionalTreatmentConsequenceEquationJoin != null
                    ? entity.ConditionalTreatmentConsequenceEquationJoin.Equation.ToDto()
                    : new EquationDTO(),
                CriterionLibrary = entity.CriterionLibraryConditionalTreatmentConsequenceJoin != null
                    ? entity.CriterionLibraryConditionalTreatmentConsequenceJoin.CriterionLibrary.ToDto()
                    : new CriterionLibraryDTO()
            };

        public static TreatmentConsequenceDTO ToDto(this ScenarioConditionalTreatmentConsequenceEntity entity) =>
            new TreatmentConsequenceDTO
            {
                Id = entity.Id,
                ChangeValue = entity.ChangeValue,
                Attribute = entity.Attribute != null
                    ? entity.Attribute.Name
                    : "",
                Equation = entity.ScenarioConditionalTreatmentConsequenceEquationJoin != null
                    ? entity.ScenarioConditionalTreatmentConsequenceEquationJoin.Equation.ToDto()
                    : new EquationDTO(),
                CriterionLibrary = entity.CriterionLibraryScenarioConditionalTreatmentConsequenceJoin != null
                    ? entity.CriterionLibraryScenarioConditionalTreatmentConsequenceJoin.CriterionLibrary.ToDto()
                    : new CriterionLibraryDTO()
            };
    }
}
