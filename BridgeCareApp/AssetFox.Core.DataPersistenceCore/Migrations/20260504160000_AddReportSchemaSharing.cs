using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AddReportSchemaSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSharedWithAll",
                table: "ReportSchema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ReportSchema_User",
                columns: table => new
                {
                    ReportSchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSchema_User", x => new { x.ReportSchemaId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReportSchema_User_ReportSchema_ReportSchemaId",
                        column: x => x.ReportSchemaId,
                        principalTable: "ReportSchema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportSchema_User_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchema_User_ReportSchemaId",
                table: "ReportSchema_User",
                column: "ReportSchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchema_User_UserId",
                table: "ReportSchema_User",
                column: "UserId");

            migrationBuilder.Sql(@"
                INSERT INTO [ReportSchema_User]
                    ([ReportSchemaId], [UserId], [AccessLevel], [CreatedDate], [LastModifiedDate], [CreatedBy], [LastModifiedBy])
                SELECT
                    rs.[Id],
                    rs.[CreatedBy],
                    2,
                    SYSUTCDATETIME(),
                    SYSUTCDATETIME(),
                    rs.[CreatedBy],
                    rs.[CreatedBy]
                FROM [ReportSchema] rs
                INNER JOIN [User] u ON u.[Id] = rs.[CreatedBy]
                WHERE rs.[CreatedBy] <> '00000000-0000-0000-0000-000000000000'
                  AND NOT EXISTS (
                    SELECT 1
                    FROM [ReportSchema_User] rsu
                    WHERE rsu.[ReportSchemaId] = rs.[Id]
                      AND rsu.[UserId] = rs.[CreatedBy]
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportSchema_User");

            migrationBuilder.DropColumn(
                name: "IsSharedWithAll",
                table: "ReportSchema");
        }
    }
}
