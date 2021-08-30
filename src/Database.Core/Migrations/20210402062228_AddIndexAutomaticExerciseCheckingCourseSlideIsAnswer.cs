using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddIndexAutomaticExerciseCheckingCourseSlideIsAnswer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			var sql =  @"
CREATE INDEX ""IX_AutomaticExerciseCheckings_CourseId_SlideId_IsRightAnswer""
ON public.""AutomaticExerciseCheckings"" USING btree
(""CourseId"", ""SlideId"", ""IsRightAnswer"") INCLUDE(""UserId"");";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AutomaticExerciseCheckings_CourseId_SlideId_IsRightAnswer",
                table: "AutomaticExerciseCheckings");
        }
    }
}
