using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class CreateUserGeneratedFlashcard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGeneratedFlashcards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(64)", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Answer = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    LastUpdateTimestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false),
                    ModeratorId = table.Column<string>(type: "character varying(64)", nullable: true)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    ModerationTimestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGeneratedFlashcards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGeneratedFlashcards_AspNetUsers_ModeratorId",
                        column: x => x.ModeratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserGeneratedFlashcards_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGeneratedFlashcards_CourseId_ModerationStatus",
                table: "UserGeneratedFlashcards",
                columns: new[] { "CourseId", "ModerationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGeneratedFlashcards_CourseId_OwnerId",
                table: "UserGeneratedFlashcards",
                columns: new[] { "CourseId", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGeneratedFlashcards_CourseId_UnitId_ModerationStatus",
                table: "UserGeneratedFlashcards",
                columns: new[] { "CourseId", "UnitId", "ModerationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGeneratedFlashcards_ModeratorId",
                table: "UserGeneratedFlashcards",
                column: "ModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGeneratedFlashcards_OwnerId",
                table: "UserGeneratedFlashcards",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGeneratedFlashcards");
        }
    }
}
