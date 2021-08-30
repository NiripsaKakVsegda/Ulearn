using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class UniqueManualCheckingBySubmission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings");

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings",
                column: "SubmissionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings");

            migrationBuilder.CreateIndex(
                name: "IX_ManualExerciseCheckings_SubmissionId",
                table: "ManualExerciseCheckings",
                column: "SubmissionId");
        }
    }
}
