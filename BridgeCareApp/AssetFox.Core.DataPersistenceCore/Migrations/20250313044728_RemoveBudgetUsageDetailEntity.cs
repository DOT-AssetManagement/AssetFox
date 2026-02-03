using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    public partial class RemoveBudgetUsageDetailEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetUsageDetail");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetUsageDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentConsiderationDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BudgetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoveredCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetUsageDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetUsageDetail_TreatmentConsiderationDetail_TreatmentConsiderationDetailId",
                        column: x => x.TreatmentConsiderationDetailId,
                        principalTable: "TreatmentConsiderationDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetUsageDetail_Id",
                table: "BudgetUsageDetail",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetUsageDetail_TreatmentConsiderationDetailId",
                table: "BudgetUsageDetail",
                column: "TreatmentConsiderationDetailId");
        }
    }
}
