using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    public partial class SimulationOutputIndexChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Handle FK dependency for IX_AssetSummaryDetail_Id ---
            migrationBuilder.Sql("ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId;");

            migrationBuilder.DropIndex(
                name: "IX_AssetSummaryDetail_Id",
                table: "AssetSummaryDetail");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.AssetSummaryDetailValueIntId
                ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId
                FOREIGN KEY (AssetSummaryDetailId) REFERENCES dbo.AssetSummaryDetail(Id) ON DELETE CASCADE;
            "); // Assuming ON DELETE CASCADE based on the other similar FK

            // --- Handle FK dependency for IX_AssetDetail_Id ---
            migrationBuilder.Sql("ALTER TABLE dbo.AssetDetailValueIntId DROP CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId;");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetail_Id",
                table: "AssetDetail");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.AssetDetailValueIntId
                ADD CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId
                FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
            "); // ON DELETE CASCADE was specified in the migration creating this FK

            // --- Drop other redundant/unnecessary indexes identified previously ---
            migrationBuilder.DropIndex(
                name: "IX_AssetSummaryDetailValueIntId_Id",
                table: "AssetSummaryDetailValueIntId");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_Id",
                table: "AssetDetailValueIntId");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_AttributeId", // Simple index
                table: "AssetDetailValueIntId");

            // Drop complex AttributeId index as confirmed unused
            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_Attribute_AttributeId", // Complex index
                table: "AssetDetailValueIntId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Recreate Indexes First ---
            migrationBuilder.CreateIndex(
               name: "IX_AssetSummaryDetail_Id",
               table: "AssetSummaryDetail",
               column: "Id",
               unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetDetail_Id",
                table: "AssetDetail",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
               name: "IX_AssetSummaryDetailValueIntId_Id",
               table: "AssetSummaryDetailValueIntId",
               column: "Id",
               unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetDetailValueIntId_Id",
                table: "AssetDetailValueIntId",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetDetailValueIntId_AttributeId", // Simple index name
                table: "AssetDetailValueIntId",
                column: "AttributeId");

            // Recreate complex AttributeId index using raw SQL for safety
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetDetailValueIntId_Attribute_AttributeId' AND object_id = OBJECT_ID('dbo.AssetDetailValueIntId'))
                CREATE NONCLUSTERED INDEX [IX_AssetDetailValueIntId_Attribute_AttributeId]
                ON [dbo].[AssetDetailValueIntId] ([AttributeId] ASC)
                INCLUDE ([AssetDetailId],[Discriminator],[TextValue],[NumericValue])
                WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
                      DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON,
                      ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
                ON [PRIMARY];
            ");

            // --- Drop and Recreate FKs to ensure they potentially re-link (or handle errors if needed) ---
            // We drop first in case they somehow automatically linked to the Clustered PK when recreated in Up()
            migrationBuilder.Sql("ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId;");
            migrationBuilder.Sql("ALTER TABLE dbo.AssetDetailValueIntId DROP CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId;");

            // Recreate FKs
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.AssetSummaryDetailValueIntId
                ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId
                FOREIGN KEY (AssetSummaryDetailId) REFERENCES dbo.AssetSummaryDetail(Id) ON DELETE CASCADE;
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.AssetDetailValueIntId
                ADD CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId
                FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
            ");
        }
    }
}
