using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Database.Migrations
{
    public partial class AddGoogleSheetExportTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleSheetExportTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    IsVisibleForStudents = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshStartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RefreshEndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RefreshTimeInMinutes = table.Column<int>(type: "integer", nullable: true),
                    SpreadsheetId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ListId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleSheetExportTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleSheetExportTasks_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoogleSheetExportTaskGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleSheetExportTaskGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleSheetExportTaskGroups_GoogleSheetExportTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "GoogleSheetExportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoogleSheetExportTaskGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetExportTaskGroups_GroupId",
                table: "GoogleSheetExportTaskGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetExportTaskGroups_TaskId",
                table: "GoogleSheetExportTaskGroups",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetExportTasks_AuthorId",
                table: "GoogleSheetExportTasks",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSheetExportTasks_CourseId_AuthorId",
                table: "GoogleSheetExportTasks",
                columns: new[] { "CourseId", "AuthorId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleSheetExportTaskGroups");

            migrationBuilder.DropTable(
                name: "GoogleSheetExportTasks");
        }
    }
}
