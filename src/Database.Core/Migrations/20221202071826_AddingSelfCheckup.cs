using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    public partial class AddingSelfCheckup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelfCheckups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    CourseId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false),
                    CheckupId = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfCheckups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SelfCheckups_CheckupId",
                table: "SelfCheckups",
                column: "CheckupId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfCheckups_UserId_CourseId_SlideId",
                table: "SelfCheckups",
                columns: new[] { "UserId", "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_SelfCheckups_UserId_CourseId_SlideId_CheckupId",
                table: "SelfCheckups",
                columns: new[] { "UserId", "CourseId", "SlideId", "CheckupId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelfCheckups");
        }
    }
}
