using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRunIdToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[AssetSummaryDetail]                DROP CONSTRAINT [FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId];
                ALTER TABLE [dbo].[SimulationYearDetail]              DROP CONSTRAINT [FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId];
                ALTER TABLE [dbo].[SimulationOutputJson]              DROP CONSTRAINT [FK_SimulationOutputJson_SimulationOutput_SimulationOutputId];
            ");

            migrationBuilder.Sql(@"
                /* empty JSON details first (child) */
                TRUNCATE TABLE [dbo].[SimulationOutputJson];

                /* then the root table */
                DELETE FROM [dbo].[SimulationOutput];
            ");

            migrationBuilder.AddColumn<int>(
               name: "RunId",
               table: "SimulationOutput",
               type: "int",
               nullable: false)
               // Specify the column should be an IDENTITY column
               .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.Sql(@"
                ALTER TABLE [dbo].[AssetSummaryDetail]
                    ADD CONSTRAINT [FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId]
                        FOREIGN KEY ([SimulationOutputId])
                        REFERENCES [dbo].[SimulationOutput] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[SimulationYearDetail]
                    ADD CONSTRAINT [FK_SimulationYearDetail_SimulationOutput_SimulationOutputId]
                        FOREIGN KEY ([SimulationOutputId])
                        REFERENCES [dbo].[SimulationOutput] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[SimulationOutputJson]
                    ADD CONSTRAINT [FK_SimulationOutputJson_SimulationOutput_SimulationOutputId]
                        FOREIGN KEY ([SimulationOutputId])
                        REFERENCES [dbo].[SimulationOutput] ([Id])
                        ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.DropColumn(
               name: "RunId",
               table: "SimulationOutput");
        }
    }
}
