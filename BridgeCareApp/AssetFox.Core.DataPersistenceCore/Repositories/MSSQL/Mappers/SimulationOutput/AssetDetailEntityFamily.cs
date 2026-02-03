using System.Collections.Generic;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL
{
    public class AssetDetailEntityFamily
    {
        public List<AssetDetailEntity> AssetDetails { get; set; } = new List<AssetDetailEntity>();

        public List<AssetDetailValueEntityIntId> AssetDetailValues { get; set; } = new List<AssetDetailValueEntityIntId>();

        public List<TreatmentOptionDetailEntity> TreatmentOptions { get; set; } = new List<TreatmentOptionDetailEntity>();

        public List<TreatmentRejectionDetailEntity> TreatmentRejections { get; set; } = new List<TreatmentRejectionDetailEntity>();

        public List<TreatmentSchedulingCollisionDetailEntity> TreatmentSchedulingCollisions { get; set; } = new List<TreatmentSchedulingCollisionDetailEntity>();

        public List<TreatmentConsiderationDetailEntity> TreatmentConsiderations { get; set; } = new List<TreatmentConsiderationDetailEntity>();
        public List<CashFlowConsiderationDetailEntity> CashFlowConsiderations { get; set; } = new List<CashFlowConsiderationDetailEntity>();
        public List<FundingCalculationInput> FundingCalculationInputs { get; set; } = new List<FundingCalculationInput>(); // single per TreatmentConsiderationDetail
        public List<BudgetToSpend> CurrentBudgetsToSpend { get; set; } = new List<BudgetToSpend>(); // list per FundingCalculationInput
        public List<FundingCalculationOutput> FundingCalculationOutputs { get; set; } = new List<FundingCalculationOutput>(); // single per TreatmentConsiderationDetail
        public List<Allocation> AllocationMatrix { get; set; } = new List<Allocation>(); // single per FundingCalculationOutput
    }
}
