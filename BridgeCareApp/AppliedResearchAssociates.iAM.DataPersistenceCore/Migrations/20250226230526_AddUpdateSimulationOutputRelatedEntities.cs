using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    public partial class AddUpdateSimulationOutputRelatedEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FundingCalculationInputId",
                table: "TreatmentConsiderationDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProjectSource",
                table: "AssetDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FundingCalculationInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentConsiderationDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingCalculationInput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundingCalculationOutput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentConsiderationDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingCalculationOutput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetToSpend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    FundingCalculationInputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetToSpend", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetToSpend_FundingCalculationInput_FundingCalculationInputId",
                        column: x => x.FundingCalculationInputId,
                        principalTable: "FundingCalculationInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Allocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BudgetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FundingCalculationOutputId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allocation_FundingCalculationOutput_FundingCalculationOutputId",
                        column: x => x.FundingCalculationOutputId,
                        principalTable: "FundingCalculationOutput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentConsiderationDetail_FundingCalculationInputId",
                table: "TreatmentConsiderationDetail",
                column: "FundingCalculationInputId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentConsiderationDetail_FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail",
                column: "FundingCalculationOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocation_FundingCalculationOutputId",
                table: "Allocation",
                column: "FundingCalculationOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetToSpend_FundingCalculationInputId",
                table: "BudgetToSpend",
                column: "FundingCalculationInputId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentConsiderationDetail_FundingCalculationInput_FundingCalculationInputId",
                table: "TreatmentConsiderationDetail",
                column: "FundingCalculationInputId",
                principalTable: "FundingCalculationInput",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentConsiderationDetail_FundingCalculationOutput_FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail",
                column: "FundingCalculationOutputId",
                principalTable: "FundingCalculationOutput",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentConsiderationDetail_FundingCalculationInput_FundingCalculationInputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentConsiderationDetail_FundingCalculationOutput_FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropTable(
                name: "Allocation");

            migrationBuilder.DropTable(
                name: "BudgetToSpend");

            migrationBuilder.DropTable(
                name: "FundingCalculationOutput");

            migrationBuilder.DropTable(
                name: "FundingCalculationInput");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentConsiderationDetail_FundingCalculationInputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentConsiderationDetail_FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropColumn(
                name: "FundingCalculationInputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropColumn(
                name: "FundingCalculationOutputId",
                table: "TreatmentConsiderationDetail");

            migrationBuilder.DropColumn(
                name: "ProjectSource",
                table: "AssetDetail");
        }
    }
}
