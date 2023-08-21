using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class GroupBaseNameDefaultCollation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                collation: "default",
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300)
                .OldAnnotation("Npgsql:DefaultColumnCollation", "case_insensitive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldCollation: "default")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");
        }
    }
}
