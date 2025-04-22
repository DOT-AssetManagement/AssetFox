using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    public partial class AddSimulationOutputStagingHeaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // helper local function
            void Clone(string live, string heap, bool needsGuidDefault) => migrationBuilder.Sql($@"
                IF OBJECT_ID('{heap}', 'U') IS NULL
                    SELECT TOP (0) * INTO {heap} FROM {live};

                TRUNCATE TABLE {heap};
                ALTER TABLE  {heap} SET (LOCK_ESCALATION = DISABLE);
                {(needsGuidDefault
                    ? $@"ALTER TABLE  {heap}
                        ADD CONSTRAINT DF_{heap.Replace("dbo.", "")}_Id
                            DEFAULT (NEWID()) FOR [Id];"
                    : "")}
                ");

            // ------------------ tables whose Id is UNIQUEIDENTIFIER ------------------
            // add NEWID() default so BulkInsert gets a non‑null key
            Clone("dbo.SimulationOutput", "dbo.SimulationOutput_Staging", true);
            Clone("dbo.AssetSummaryDetail", "dbo.AssetSummaryDetail_Staging", true);
            Clone("dbo.SimulationYearDetail", "dbo.SimulationYearDetail_Staging", true);
            Clone("dbo.AssetDetail", "dbo.AssetDetail_Staging", true);
            Clone("dbo.TreatmentOptionDetail", "dbo.TreatmentOptionDetail_Staging", true);
            Clone("dbo.TreatmentRejectionDetail", "dbo.TreatmentRejectionDetail_Staging", true);
            Clone("dbo.TreatmentSchedulingCollisionDetail", "dbo.TreatmentSchedulingCollisionDetail_Staging", true);
            Clone("dbo.TreatmentConsiderationDetail", "dbo.TreatmentConsiderationDetail_Staging", true);
            Clone("dbo.FundingCalculationInput", "dbo.FundingCalculationInput_Staging", true);
            Clone("dbo.FundingCalculationOutput", "dbo.FundingCalculationOutput_Staging", true);
            Clone("dbo.BudgetToSpend", "dbo.BudgetToSpend_Staging", true);
            Clone("dbo.Allocation", "dbo.Allocation_Staging", true);
            Clone("dbo.CashFlowConsiderationDetail", "dbo.CashFlowConsiderationDetail_Staging", true);

            // ------------------ tables whose Id is INT IDENTITY ------------------
            // no default constraint needed (identity supplies the value)
            Clone("dbo.AssetSummaryDetailValueIntId", "dbo.AssetSummaryDetailValueIntId_Staging", false);
            Clone("dbo.AssetDetailValueIntId", "dbo.AssetDetailValueIntId_Staging", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string[] heaps =
            {
                "SimulationOutput","AssetSummaryDetail","AssetSummaryDetailValueIntId",
                "SimulationYearDetail","AssetDetail","AssetDetailValueIntId",
                "TreatmentOptionDetail","TreatmentRejectionDetail","TreatmentSchedulingCollisionDetail",
                "TreatmentConsiderationDetail","FundingCalculationInput","FundingCalculationOutput",
                "BudgetToSpend","Allocation","CashFlowConsiderationDetail"
            };
                foreach (var t in heaps)
                    migrationBuilder.Sql($"DROP TABLE IF EXISTS dbo.{t}_Staging;");
        }
    }
}
