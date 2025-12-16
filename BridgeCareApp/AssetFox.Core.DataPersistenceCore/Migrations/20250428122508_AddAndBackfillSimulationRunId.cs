using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAndBackfillSimulationRunId : Migration
    {
        private readonly string[] _tablesToUpdate = {
        "SimulationYearDetail", "AssetSummaryDetail", "AssetDetail",
        "AssetSummaryDetailValueIntId", "AssetDetailValueIntId",
        "TreatmentOptionDetail", "TreatmentRejectionDetail", "TreatmentSchedulingCollisionDetail",
        "TreatmentConsiderationDetail", "FundingCalculationInput", "FundingCalculationOutput",
        "BudgetToSpend", "Allocation", "CashFlowConsiderationDetail", "TargetConditionGoalDetail", "DeficientConditionGoalDetail", "BudgetDetail"
        // Add any other relevant tables
    };

        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // Add nullable column to all tables
            foreach (var table in _tablesToUpdate)
            {
                migrationBuilder.AddColumn<int>(
                    name: "RunId",
                    table: table, // Assumes dbo schema, add schema if needed
                    type: "int",
                    nullable: false); // Add as NULLable first
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Simple rollback: just drop the columns
            foreach (var table in _tablesToUpdate.Reverse()) // Drop in reverse order
            {
                migrationBuilder.DropColumn(
                   name: "RunId",
                   table: table);
            }
        }
    }
}
