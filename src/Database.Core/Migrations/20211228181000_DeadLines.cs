using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class DeadLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeadLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScorePercent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLines_CourseId_GroupId",
                table: "DeadLines",
                columns: new[] { "CourseId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLines_CourseId_GroupId_UnitId_SlideId_UserId",
                table: "DeadLines",
                columns: new[] { "CourseId", "GroupId", "UnitId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLines_CourseId_UserId",
                table: "DeadLines",
                columns: new[] { "CourseId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeadLines");
        }
    }
}
