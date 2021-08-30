using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class NamesToLower : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Names",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(immutable_concat_ws(' ', nullif(\"UserName\", ''), nullif(\"FirstName\",''), nullif(\"LastName\",''), nullif(\"FirstName\",'')))",
                stored: true,
                collation: "default",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComputedColumnSql: "immutable_concat_ws(' ', nullif(\"UserName\", ''), nullif(\"FirstName\",''), nullif(\"LastName\",''), nullif(\"FirstName\",''))",
                oldStored: true,
                oldCollation: "default");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Names",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                computedColumnSql: "immutable_concat_ws(' ', nullif(\"UserName\", ''), nullif(\"FirstName\",''), nullif(\"LastName\",''), nullif(\"FirstName\",''))",
                stored: true,
                collation: "default",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComputedColumnSql: "lower(immutable_concat_ws(' ', nullif(\"UserName\", ''), nullif(\"FirstName\",''), nullif(\"LastName\",''), nullif(\"FirstName\",'')))",
                oldStored: true,
                oldCollation: "default");
        }
    }
}
