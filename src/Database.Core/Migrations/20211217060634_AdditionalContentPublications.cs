using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AdditionalContentPublications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalContentPublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalContentPublications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalContentPublications_CourseId_GroupId",
                table: "AdditionalContentPublications",
                columns: new[] { "CourseId", "GroupId" });
			
			migrationBuilder.CreateIndex(
				name: "IX_AdditionalContentPublications_CourseId_GroupId_UnitId_SlideId",
				table: "AdditionalContentPublications",
				columns: new[] { "CourseId", "GroupId", "UnitId", "SlideId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalContentPublications");
        }
    }
}
