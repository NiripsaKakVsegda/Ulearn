using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AllowExerciseCodeReviewsCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~",
                table: "ExerciseCodeReviews");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~",
                table: "ExerciseCodeReviews",
                column: "ExerciseCheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~",
                table: "ExerciseCodeReviews");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseCodeReviews_ManualExerciseCheckings_ExerciseCheckin~",
                table: "ExerciseCodeReviews",
                column: "ExerciseCheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
