using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class CleanUpOldCommittedProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE CommittedProject
                SET name = '[""' + name + '""]'
                WHERE
                    name IS NOT NULL
                    AND (
                        LEFT(name, 1) <> '['
                        OR RIGHT(name, 1) <> ']'
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
