using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCourseAndSlideToLikesAndPromotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "Likes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<Guid>(
                name: "SlideId",
                table: "Likes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CourseId",
                table: "AcceptedSolutionsPromotes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<Guid>(
                name: "SlideId",
                table: "AcceptedSolutionsPromotes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Likes_CourseId_SlideId_SubmissionId",
                table: "Likes",
                columns: new[] { "CourseId", "SlideId", "SubmissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AcceptedSolutionsPromotes_CourseId_SlideId",
                table: "AcceptedSolutionsPromotes",
                columns: new[] { "CourseId", "SlideId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Likes_CourseId_SlideId_SubmissionId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_AcceptedSolutionsPromotes_CourseId_SlideId",
                table: "AcceptedSolutionsPromotes");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "SlideId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "AcceptedSolutionsPromotes");

            migrationBuilder.DropColumn(
                name: "SlideId",
                table: "AcceptedSolutionsPromotes");
        }
    }
}
