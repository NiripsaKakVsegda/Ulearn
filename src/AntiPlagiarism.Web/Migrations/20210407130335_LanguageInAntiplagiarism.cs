using Microsoft.EntityFrameworkCore.Migrations;

namespace AntiPlagiarism.Web.Migrations
{
    public partial class LanguageInAntiplagiarism : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TasksStatisticsParameters",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClientId_TaskId",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClientId_TaskId_AddingTime_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClientId_TaskId_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_SnippetsStatistics_SnippetId_TaskId_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManualSuspicionLevels",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels");

            migrationBuilder.AddColumn<short>(
                name: "Language",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Language",
                schema: "antiplagiarism",
                table: "SnippetsStatistics",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Language",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TasksStatisticsParameters",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters",
                columns: new[] { "TaskId", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManualSuspicionLevels",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels",
                columns: new[] { "TaskId", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_Language",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsStatistics_SnippetId_TaskId_Language_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics",
                columns: new[] { "SnippetId", "TaskId", "Language", "ClientId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TasksStatisticsParameters",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClientId_TaskId_Language",
                schema: "antiplagiarism",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_SnippetsStatistics_SnippetId_TaskId_Language_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManualSuspicionLevels",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "antiplagiarism",
                table: "SnippetsStatistics");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TasksStatisticsParameters",
                schema: "antiplagiarism",
                table: "TasksStatisticsParameters",
                column: "TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManualSuspicionLevels",
                schema: "antiplagiarism",
                table: "ManualSuspicionLevels",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AddingTime_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AddingTime", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClientId_TaskId_AuthorId",
                schema: "antiplagiarism",
                table: "Submissions",
                columns: new[] { "ClientId", "TaskId", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnippetsStatistics_SnippetId_TaskId_ClientId",
                schema: "antiplagiarism",
                table: "SnippetsStatistics",
                columns: new[] { "SnippetId", "TaskId", "ClientId" },
                unique: true);
        }
    }
}
