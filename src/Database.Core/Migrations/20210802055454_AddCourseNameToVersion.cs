using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddCourseNameToVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "CourseVersions",
                type: "text",
                nullable: false,
                defaultValue: "")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "CourseVersions");
        }
    }
}
