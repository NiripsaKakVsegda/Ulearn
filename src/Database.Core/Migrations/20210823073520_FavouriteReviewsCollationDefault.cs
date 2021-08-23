using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class FavouriteReviewsCollationDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "FavouriteReviews",
                type: "text",
                nullable: false,
                collation: "default",
                oldClrType: typeof(string),
                oldType: "text")
                .OldAnnotation("Npgsql:DefaultColumnCollation", "case_insensitive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "FavouriteReviews",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "default")
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");
        }
    }
}
