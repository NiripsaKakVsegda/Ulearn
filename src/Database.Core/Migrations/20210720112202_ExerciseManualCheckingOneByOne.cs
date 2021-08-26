using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Database.Migrations
{
    public partial class ExerciseManualCheckingOneByOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId",
                table: "ManualExerciseCheckings");

            migrationBuilder.DropIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "ManualExerciseCheckings");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ManualExerciseCheckings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_ManualExerciseCheckings_UserExerciseSubmissions_Id",
                table: "ManualExerciseCheckings",
                column: "Id",
                principalTable: "UserExerciseSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManualExerciseCheckings_UserExerciseSubmissions_Id",
                table: "ManualExerciseCheckings");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ManualExerciseCheckings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionId",
                table: "ManualExerciseCheckings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ManualExerciseCheckings_UserExerciseSubmissions_SubmissionId",
                table: "ManualExerciseCheckings",
                column: "SubmissionId",
                principalTable: "UserExerciseSubmissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
