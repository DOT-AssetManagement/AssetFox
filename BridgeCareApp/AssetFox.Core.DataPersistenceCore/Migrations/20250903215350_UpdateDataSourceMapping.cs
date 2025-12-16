using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataSourceMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DataSourceMapping_AttributeId",
                table: "DataSourceMapping",
                column: "AttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSourceMapping_Attribute_AttributeId",
                table: "DataSourceMapping",
                column: "AttributeId",
                principalTable: "Attribute",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSourceMapping_Attribute_AttributeId",
                table: "DataSourceMapping");

            migrationBuilder.DropIndex(
                name: "IX_DataSourceMapping_AttributeId",
                table: "DataSourceMapping");
        }
    }
}
