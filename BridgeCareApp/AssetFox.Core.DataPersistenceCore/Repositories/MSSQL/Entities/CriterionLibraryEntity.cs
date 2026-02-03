using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.Abstract;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.BudgetPriority;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.PerformanceCurve;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CalculatedAttribute;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Budget;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.BudgetPriority;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.PerformanceCurve;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CalculatedAttribute;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Treatment;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.CashFlow;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.Deficient;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.RemainingLifeLimit;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.LibraryEntities.TargetConditionGoal;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.CashFlow;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.Deficient;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.RemainingLifeLimit;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities.ScenarioEntities.TargetConditionGoal;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class CriterionLibraryEntity : LibraryEntity
    {
        public CriterionLibraryEntity()
        {
            CriterionLibraryAnalysisMethodJoins = new HashSet<CriterionLibraryAnalysisMethodEntity>();
            CriterionLibraryBudgetJoins = new HashSet<CriterionLibraryBudgetEntity>();
            CriterionLibraryScenarioBudgetJoins = new HashSet<CriterionLibraryScenarioBudgetEntity>();
            CriterionLibraryBudgetPriorityJoins = new HashSet<CriterionLibraryBudgetPriorityEntity>();
            CriterionLibraryScenarioBudgetPriorityJoins = new HashSet<CriterionLibraryScenarioBudgetPriorityEntity>();
            CriterionLibraryCashFlowRuleJoins = new HashSet<CriterionLibraryCashFlowRuleEntity>();
            CriterionLibraryScenarioCashFlowRuleJoins = new HashSet<CriterionLibraryScenarioCashFlowRuleEntity>();
            CriterionLibraryDeficientConditionGoalJoins = new HashSet<CriterionLibraryDeficientConditionGoalEntity>();
            CriterionLibraryScenarioDeficientConditionGoalJoins = new HashSet<CriterionLibraryScenarioDeficientConditionGoalEntity>();
            CriterionLibraryPerformanceCurveJoins = new HashSet<CriterionLibraryPerformanceCurveEntity>();
            CriterionLibraryScenarioPerformanceCurveJoins = new HashSet<CriterionLibraryScenarioPerformanceCurveEntity>();
            CriterionLibraryCalculatedAttributePairJoins = new HashSet<CriterionLibraryCalculatedAttributePairEntity>();
            CriterionLibraryScenarioCalculatedAttributePairJoins = new HashSet<ScenarioCriterionLibraryCalculatedAttributePairEntity>();
            CriterionLibraryRemainingLifeLimitJoins = new HashSet<CriterionLibraryRemainingLifeLimitEntity>();
            CriterionLibraryScenarioRemainingLifeLimitJoins = new HashSet<CriterionLibraryScenarioRemainingLifeLimitEntity>();
            CriterionLibrarySelectableTreatmentJoins = new HashSet<CriterionLibrarySelectableTreatmentEntity>();
            CriterionLibraryScenarioSelectableTreatmentJoins = new HashSet<CriterionLibraryScenarioSelectableTreatmentEntity>();
            CriterionLibraryTreatmentConsequenceJoins = new HashSet<CriterionLibraryConditionalTreatmentConsequenceEntity>();
            CriterionLibraryTargetConditionGoalJoins = new HashSet<CriterionLibraryTargetConditionGoalEntity>();
            CriterionLibraryScenarioTargetConditionGoalJoins = new HashSet<CriterionLibraryScenarioTargetConditionGoalEntity>();
            CriterionLibraryScenarioTreatmentConsequenceJoins = new HashSet<CriterionLibraryScenarioConditionalTreatmentConsequenceEntity>();
            CriterionLibraryTreatmentCostJoins = new HashSet<CriterionLibraryTreatmentCostEntity>();
            CriterionLibraryScenarioTreatmentCostJoins = new HashSet<CriterionLibraryScenarioTreatmentCostEntity>();
            CriterionLibraryTreatmentSupersedeRuleJoins = new HashSet<CriterionLibraryTreatmentSupersedeRuleEntity>();
            CriterionLibraryScenarioTreatmentSupersedeRuleJoins = new HashSet<CriterionLibraryScenarioTreatmentSupersedeRuleEntity>();
            AttributeEquationCriterionLibraryJoins = new HashSet<AttributeEquationCriterionLibraryEntity>();
            CriterionLibraryUserJoins = new HashSet<CriterionLibraryUserEntity>();
        }

        public string MergedCriteriaExpression { get; set; }

        public bool IsSingleUse { get; set; }

        public virtual ICollection<CriterionLibraryAnalysisMethodEntity> CriterionLibraryAnalysisMethodJoins { get; set; }

        public virtual ICollection<CriterionLibraryBudgetEntity> CriterionLibraryBudgetJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioBudgetEntity> CriterionLibraryScenarioBudgetJoins { get; set; }

        public virtual ICollection<CriterionLibraryBudgetPriorityEntity> CriterionLibraryBudgetPriorityJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioBudgetPriorityEntity> CriterionLibraryScenarioBudgetPriorityJoins { get; set; }

        public virtual ICollection<CriterionLibraryCashFlowRuleEntity> CriterionLibraryCashFlowRuleJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioCashFlowRuleEntity> CriterionLibraryScenarioCashFlowRuleJoins { get; set; }

        public virtual ICollection<CriterionLibraryDeficientConditionGoalEntity> CriterionLibraryDeficientConditionGoalJoins { get; set; }
        public virtual ICollection<CriterionLibraryScenarioDeficientConditionGoalEntity> CriterionLibraryScenarioDeficientConditionGoalJoins { get; set; }

        public virtual ICollection<CriterionLibraryPerformanceCurveEntity> CriterionLibraryPerformanceCurveJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioPerformanceCurveEntity> CriterionLibraryScenarioPerformanceCurveJoins { get; set; }

        public virtual ICollection<CriterionLibraryCalculatedAttributePairEntity> CriterionLibraryCalculatedAttributePairJoins { get; set; }

        public virtual ICollection<ScenarioCriterionLibraryCalculatedAttributePairEntity> CriterionLibraryScenarioCalculatedAttributePairJoins { get; set; }

        public virtual ICollection<CriterionLibraryRemainingLifeLimitEntity> CriterionLibraryRemainingLifeLimitJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioRemainingLifeLimitEntity> CriterionLibraryScenarioRemainingLifeLimitJoins { get; set; }

        public virtual ICollection<CriterionLibraryTargetConditionGoalEntity> CriterionLibraryTargetConditionGoalJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioTargetConditionGoalEntity> CriterionLibraryScenarioTargetConditionGoalJoins { get; set; }

        public virtual ICollection<CriterionLibrarySelectableTreatmentEntity> CriterionLibrarySelectableTreatmentJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioSelectableTreatmentEntity> CriterionLibraryScenarioSelectableTreatmentJoins { get; set; }

        public virtual ICollection<CriterionLibraryConditionalTreatmentConsequenceEntity> CriterionLibraryTreatmentConsequenceJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioConditionalTreatmentConsequenceEntity> CriterionLibraryScenarioTreatmentConsequenceJoins { get; set; }

        public virtual ICollection<CriterionLibraryTreatmentCostEntity> CriterionLibraryTreatmentCostJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioTreatmentCostEntity> CriterionLibraryScenarioTreatmentCostJoins { get; set; }

        public virtual ICollection<CriterionLibraryTreatmentSupersedeRuleEntity> CriterionLibraryTreatmentSupersedeRuleJoins { get; set; }

        public virtual ICollection<CriterionLibraryScenarioTreatmentSupersedeRuleEntity> CriterionLibraryScenarioTreatmentSupersedeRuleJoins { get; set; }

        public virtual ICollection<AttributeEquationCriterionLibraryEntity> AttributeEquationCriterionLibraryJoins { get; set; }

        public virtual ICollection<CriterionLibraryUserEntity> CriterionLibraryUserJoins { get; set; }
    }
}
