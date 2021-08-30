using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AllowNotificationsCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_AddedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_InitiatedById",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_JoinedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_LikedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_ParentCommentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CourseVersions_CourseVersionId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_GroupAccesses_AccessId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ManualExerciseCheckings_CheckingId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_StepikExportProcesses_ProcessId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_AddedUserId",
                table: "Notifications",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_InitiatedById",
                table: "Notifications",
                column: "InitiatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_JoinedUserId",
                table: "Notifications",
                column: "JoinedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_LikedUserId",
                table: "Notifications",
                column: "LikedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications",
                column: "CommentId1",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_ParentCommentId",
                table: "Notifications",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CourseVersions_CourseVersionId",
                table: "Notifications",
                column: "CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~",
                table: "Notifications",
                column: "UploadedPackageNotification_CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_GroupAccesses_AccessId",
                table: "Notifications",
                column: "AccessId",
                principalTable: "GroupAccesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~",
                table: "Notifications",
                column: "RevokedAccessToGroupNotification_AccessId",
                principalTable: "GroupAccesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId",
                table: "Notifications",
                column: "JoinedToYourGroupNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ManualExerciseCheckings_CheckingId",
                table: "Notifications",
                column: "CheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~",
                table: "Notifications",
                column: "PassedManualQuizCheckingNotification_CheckingId",
                principalTable: "ManualQuizCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_StepikExportProcesses_ProcessId",
                table: "Notifications",
                column: "ProcessId",
                principalTable: "StepikExportProcesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_AddedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_InitiatedById",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_JoinedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_LikedUserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_ParentCommentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CourseVersions_CourseVersionId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_GroupAccesses_AccessId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ManualExerciseCheckings_CheckingId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_StepikExportProcesses_ProcessId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_AddedUserId",
                table: "Notifications",
                column: "AddedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_InitiatedById",
                table: "Notifications",
                column: "InitiatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_JoinedUserId",
                table: "Notifications",
                column: "JoinedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_LikedUserId",
                table: "Notifications",
                column: "LikedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications",
                column: "CommentId1",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_ParentCommentId",
                table: "Notifications",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CourseVersions_CourseVersionId",
                table: "Notifications",
                column: "CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CourseVersions_UploadedPackageNotification_Co~",
                table: "Notifications",
                column: "UploadedPackageNotification_CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_GroupAccesses_AccessId",
                table: "Notifications",
                column: "AccessId",
                principalTable: "GroupAccesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_GroupAccesses_RevokedAccessToGroupNotificatio~",
                table: "Notifications",
                column: "RevokedAccessToGroupNotification_AccessId",
                principalTable: "GroupAccesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_GroupId1",
                table: "Notifications",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Groups_JoinedToYourGroupNotification_GroupId",
                table: "Notifications",
                column: "JoinedToYourGroupNotification_GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ManualExerciseCheckings_CheckingId",
                table: "Notifications",
                column: "CheckingId",
                principalTable: "ManualExerciseCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ManualQuizCheckings_PassedManualQuizCheckingN~",
                table: "Notifications",
                column: "PassedManualQuizCheckingNotification_CheckingId",
                principalTable: "ManualQuizCheckings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_StepikExportProcesses_ProcessId",
                table: "Notifications",
                column: "ProcessId",
                principalTable: "StepikExportProcesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
