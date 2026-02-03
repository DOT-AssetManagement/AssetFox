using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAssetAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldUseSuperAssets",
                table: "AnalysisMethod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SuperAssetAttribute",
                table: "AnalysisMethod",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldUseSuperAssets",
                table: "AnalysisMethod");

            migrationBuilder.DropColumn(
                name: "SuperAssetAttribute",
                table: "AnalysisMethod");
        }
    }
}
