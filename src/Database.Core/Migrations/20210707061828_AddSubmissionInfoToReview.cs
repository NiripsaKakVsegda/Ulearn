using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddSubmissionInfoToReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "ExerciseCodeReviews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<Guid>(
                name: "SlideId",
                table: "ExerciseCodeReviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SubmissionAuthorId",
                table: "ExerciseCodeReviews",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_CourseId_SlideId_SubmissionAuthorId",
                table: "ExerciseCodeReviews",
                columns: new[] { "CourseId", "SlideId", "SubmissionAuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseCodeReviews_SubmissionAuthorId",
                table: "ExerciseCodeReviews",
                column: "SubmissionAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_AspNetUsers_SubmissionAuthorId",
                table: "ExerciseCodeReviews",
                column: "SubmissionAuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_AspNetUsers_SubmissionAuthorId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseCodeReviews_CourseId_SlideId_SubmissionAuthorId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseCodeReviews_SubmissionAuthorId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropColumn(
                name: "SlideId",
                table: "ExerciseCodeReviews");

            migrationBuilder.DropColumn(
                name: "SubmissionAuthorId",
                table: "ExerciseCodeReviews");
        }
    }
}
