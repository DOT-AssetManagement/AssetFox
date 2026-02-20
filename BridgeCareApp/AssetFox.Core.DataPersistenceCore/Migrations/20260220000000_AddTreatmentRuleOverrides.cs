using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentRuleOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentRuleOverride",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentLibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabledByDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentRuleOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentRuleOverride_TreatmentLibrary_TreatmentLibraryId",
                        column: x => x.TreatmentLibraryId,
                        principalTable: "TreatmentLibrary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentRuleOverrideCriterion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentRuleOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CriterionExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentRuleOverrideCriterion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentRuleOverrideCriterion_TreatmentRuleOverride_TreatmentRuleOverrideId",
                        column: x => x.TreatmentRuleOverrideId,
                        principalTable: "TreatmentRuleOverride",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioTreatmentRuleOverrideSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SimulationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverrideName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTreatmentRuleOverrideSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioTreatmentRuleOverrideSetting_Simulation_SimulationId",
                        column: x => x.SimulationId,
                        principalTable: "Simulation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRuleOverride_TreatmentLibraryId_Name",
                table: "TreatmentRuleOverride",
                columns: new[] { "TreatmentLibraryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentRuleOverrideCriterion_TreatmentRuleOverrideId",
                table: "TreatmentRuleOverrideCriterion",
                column: "TreatmentRuleOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTreatmentRuleOverrideSetting_SimulationId_OverrideName",
                table: "ScenarioTreatmentRuleOverrideSetting",
                columns: new[] { "SimulationId", "OverrideName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TreatmentRuleOverrideCriterion");
            migrationBuilder.DropTable(name: "TreatmentRuleOverride");
            migrationBuilder.DropTable(name: "ScenarioTreatmentRuleOverrideSetting");
        }
    }
}
