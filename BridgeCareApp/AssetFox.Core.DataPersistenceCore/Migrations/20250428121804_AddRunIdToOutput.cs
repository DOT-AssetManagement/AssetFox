using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRunIdToOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
