using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class FixNotificationsColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId2",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId3",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId4",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId2",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupMemberHasBeenRemovedNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "GroupMemberHasBeenRemovedNotification_GroupId",
                table: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_GroupId3",
                table: "Notifications",
                newName: "IX_Notifications_GroupId");

/*Уже есть в базе
            migrationBuilder.AddColumn<string>(
                name: "Text1",
                table: "Notifications",
                type: "text",
                nullable: true)
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");
                
            migrationBuilder.AddColumn<int>(
                name: "GroupId1",
                table: "Notifications",
                type: "integer",
                nullable: true);
*/

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications",
                column: "GroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications");

/* Уже есть в базе
            migrationBuilder.DropColumn(
                name: "Text1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "Notifications");
*/

            migrationBuilder.DropIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications",
                newName: "IX_Notifications_GroupId3");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "character varying(64)",
                nullable: true)
                .Annotation("Npgsql:DefaultColumnCollation", "case_insensitive");

            migrationBuilder.AddColumn<int>(
                name: "GroupMemberHasBeenRemovedNotification_GroupId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId1",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId2",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupMemberHasBeenRemovedNotification_GroupId",
                table: "Notifications",
                column: "GroupMemberHasBeenRemovedNotification_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId2",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId3",
                table: "Notifications",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId4",
                table: "Notifications",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupMemberHasBeenRemovedNotification_~",
                table: "Notifications",
                column: "GroupMemberHasBeenRemovedNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
