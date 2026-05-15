using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentRuleOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScenarioTreatmentRuleOverride",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SimulationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTreatmentRuleOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTreatmentRuleOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioTreatmentRuleOverride_Simulation_SimulationId",
                        column: x => x.SimulationId,
                        principalTable: "Simulation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentRuleOverride",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentLibraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabledByDefault = table.Column<bool>(type: "bit", nullable: false),
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
                name: "ScenarioTreatmentRuleOverrideCriterion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioTreatmentRuleOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogicalOperator = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CriterionExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTreatmentRuleOverrideCriterion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioTreatmentRuleOverrideCriterion_ScenarioTreatmentRuleOverride_ScenarioTreatmentRuleOverrideId",
                        column: x => x.ScenarioTreatmentRuleOverrideId,
                        principalTable: "ScenarioTreatmentRuleOverride",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentRuleOverrideCriterion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentRuleOverrideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogicalOperator = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                name: "ScenarioTreatmentRuleOverrideCriterion_Treatment",
                columns: table => new
                {
                    ScenarioTreatmentRuleOverrideCriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScenarioSelectableTreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioTreatmentRuleOverrideCriterion_Treatment", x => new { x.ScenarioTreatmentRuleOverrideCriterionId, x.ScenarioSelectableTreatmentId });
                    table.ForeignKey(
                        name: "FK_ScenarioTreatmentRuleOverrideCriterion_Treatment_ScenarioSelectableTreatment_ScenarioSelectableTreatmentId",
                        column: x => x.ScenarioSelectableTreatmentId,
                        principalTable: "ScenarioSelectableTreatment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioTreatmentRuleOverrideCriterion_Treatment_ScenarioTreatmentRuleOverrideCriterion_ScenarioTreatmentRuleOverrideCriteri~",
                        column: x => x.ScenarioTreatmentRuleOverrideCriterionId,
                        principalTable: "ScenarioTreatmentRuleOverrideCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentRuleOverrideCriterion_Treatment",
                columns: table => new
                {
                    TreatmentRuleOverrideCriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectableTreatmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentRuleOverrideCriterion_Treatment", x => new { x.TreatmentRuleOverrideCriterionId, x.SelectableTreatmentId });
                    table.ForeignKey(
                        name: "FK_TreatmentRuleOverrideCriterion_Treatment_SelectableTreatment_SelectableTreatmentId",
                        column: x => x.SelectableTreatmentId,
                        principalTable: "SelectableTreatment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TreatmentRuleOverrideCriterion_Treatment_TreatmentRuleOverrideCriterion_TreatmentRuleOverrideCriterionId",
                        column: x => x.TreatmentRuleOverrideCriterionId,
                        principalTable: "TreatmentRuleOverrideCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTreatmentRuleOverride_SimulationId_Name",
                table: "ScenarioTreatmentRuleOverride",
                columns: new[] { "SimulationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTreatmentRuleOverride_SourceTreatmentRuleOverrideId",
                table: "ScenarioTreatmentRuleOverride",
                column: "SourceTreatmentRuleOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTreatmentRuleOverrideCriterion_ScenarioTreatmentRuleOverrideId",
                table: "ScenarioTreatmentRuleOverrideCriterion",
                column: "ScenarioTreatmentRuleOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioTreatmentRuleOverrideCriterion_Treatment_ScenarioSelectableTreatmentId",
                table: "ScenarioTreatmentRuleOverrideCriterion_Treatment",
                column: "ScenarioSelectableTreatmentId");

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
                name: "IX_TreatmentRuleOverrideCriterion_Treatment_SelectableTreatmentId",
                table: "TreatmentRuleOverrideCriterion_Treatment",
                column: "SelectableTreatmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenarioTreatmentRuleOverrideCriterion_Treatment");

            migrationBuilder.DropTable(
                name: "TreatmentRuleOverrideCriterion_Treatment");

            migrationBuilder.DropTable(
                name: "ScenarioTreatmentRuleOverrideCriterion");

            migrationBuilder.DropTable(
                name: "TreatmentRuleOverrideCriterion");

            migrationBuilder.DropTable(
                name: "ScenarioTreatmentRuleOverride");

            migrationBuilder.DropTable(
                name: "TreatmentRuleOverride");
        }
    }
}
