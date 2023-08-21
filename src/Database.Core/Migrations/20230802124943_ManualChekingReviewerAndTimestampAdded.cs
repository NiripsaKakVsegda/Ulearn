using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class ManualChekingReviewerAndTimestampAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckedById",
                table: "ManualQuizCheckings",
                type: "character varying(64)",
                nullable: true)
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedTimestamp",
                table: "ManualQuizCheckings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckedById",
                table: "ManualExerciseCheckings",
                type: "character varying(64)",
                nullable: true)
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedTimestamp",
                table: "ManualExerciseCheckings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManualQuizCheckings_CheckedById",
                table: "ManualQuizCheckings",
                column: "CheckedById");

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_CheckedById",
                table: "ManualExerciseCheckings",
                column: "CheckedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ManualExerciseCheckings_AspNetUsers_CheckedById",
                table: "ManualExerciseCheckings",
                column: "CheckedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ManualQuizCheckings_AspNetUsers_CheckedById",
                table: "ManualQuizCheckings",
                column: "CheckedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManualExerciseCheckings_AspNetUsers_CheckedById",
                table: "ManualExerciseCheckings");

            migrationBuilder.DropForeignKey(
                name: "FK_ManualQuizCheckings_AspNetUsers_CheckedById",
                table: "ManualQuizCheckings");

            migrationBuilder.DropIndex(
                name: "IX_ManualQuizCheckings_CheckedById",
                table: "ManualQuizCheckings");

            migrationBuilder.DropIndex(
                name: "IX_ManualExerciseCheckings_CheckedById",
                table: "ManualExerciseCheckings");

            migrationBuilder.DropColumn(
                name: "CheckedById",
                table: "ManualQuizCheckings");

            migrationBuilder.DropColumn(
                name: "CheckedTimestamp",
                table: "ManualQuizCheckings");

            migrationBuilder.DropColumn(
                name: "CheckedById",
                table: "ManualExerciseCheckings");

            migrationBuilder.DropColumn(
                name: "CheckedTimestamp",
                table: "ManualExerciseCheckings");
        }
    }
}
