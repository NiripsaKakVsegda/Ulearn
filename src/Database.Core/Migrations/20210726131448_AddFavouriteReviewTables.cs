using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Database.Migrations
{
    public partial class AddFavouriteReviewTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavouriteReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteReviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FavouriteReviewsByUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FavouriteReviewId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    SlideId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                        .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteReviewsByUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavouriteReviewsByUsers_FavouriteReviews_FavouriteReviewId",
                        column: x => x.FavouriteReviewId,
                        principalTable: "FavouriteReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteReviews_CourseId_SlideId",
                table: "FavouriteReviews",
                columns: new[] { "CourseId", "SlideId" });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteReviews_Text",
                table: "FavouriteReviews",
                column: "Text",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteReviewsByUsers_CourseId_SlideId_Timestamp",
                table: "FavouriteReviewsByUsers",
                columns: new[] { "CourseId", "SlideId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteReviewsByUsers_CourseId_SlideId_UserId",
                table: "FavouriteReviewsByUsers",
                columns: new[] { "CourseId", "SlideId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteReviewsByUsers_FavouriteReviewId",
                table: "FavouriteReviewsByUsers",
                column: "FavouriteReviewId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavouriteReviewsByUsers");

            migrationBuilder.DropTable(
                name: "FavouriteReviews");
        }
    }
}
