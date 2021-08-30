using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class ChangeVisitsIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_SlideId_Timestamp",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_SlideId_UserId",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CourseId_SlideId_Timestamp",
                table: "Visits",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_CourseId_SlideId_Timestamp",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SlideId_Timestamp",
                table: "Visits",
                columns: new[] { "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SlideId_UserId",
                table: "Visits",
                columns: new[] { "SlideId", "UserId" });
        }
    }
}
