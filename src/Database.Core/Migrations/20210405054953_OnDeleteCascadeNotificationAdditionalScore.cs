using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class OnDeleteCascadeNotificationAdditionalScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AdditionalScores_ScoreId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AdditionalScores_ScoreId",
                table: "Notifications",
                column: "ScoreId",
                principalTable: "AdditionalScores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AdditionalScores_ScoreId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AdditionalScores_ScoreId",
                table: "Notifications",
                column: "ScoreId",
                principalTable: "AdditionalScores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
