using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddGroupIsArchivedNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupIsArchivedNotification_GroupId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupIsArchivedNotification_GroupId",
                table: "Notifications",
                column: "GroupIsArchivedNotification_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupIsArchivedNotification_GroupId",
                table: "Notifications",
                column: "GroupIsArchivedNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupIsArchivedNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupIsArchivedNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "GroupIsArchivedNotification_GroupId",
                table: "Notifications");
        }
    }
}
