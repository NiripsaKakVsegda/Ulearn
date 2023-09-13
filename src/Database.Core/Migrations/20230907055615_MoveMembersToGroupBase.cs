using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class MoveMembersToGroupBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// No database updates
			// Navigation properites in snapshot updated
			// Members moved from SingleGroup to GroupBase
			// SingleGroup replaced with GroupBase in GroupMember
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
