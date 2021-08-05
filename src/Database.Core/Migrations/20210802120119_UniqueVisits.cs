using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class UniqueVisits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_CourseId_SlideId_UserId",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CourseId_SlideId_UserId",
                table: "Visits",
                columns: new[] { "CourseId", "SlideId", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_CourseId_SlideId_UserId",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CourseId_SlideId_UserId",
                table: "Visits",
                columns: new[] { "CourseId", "SlideId", "UserId" });
        }
    }
}
