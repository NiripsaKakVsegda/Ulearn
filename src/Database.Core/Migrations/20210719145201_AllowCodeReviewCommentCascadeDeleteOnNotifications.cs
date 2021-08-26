using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AllowCodeReviewCommentCascadeDeleteOnNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications",
                column: "CommentId",
                principalTable: "ExerciseCodeReviewComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ExerciseCodeReviewComments_CommentId",
                table: "Notifications",
                column: "CommentId",
                principalTable: "ExerciseCodeReviewComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
