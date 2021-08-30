using Database.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddExerciseStatisticsViews : Migration
	{
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			var sql = @$"
CREATE MATERIALIZED VIEW public.""{ExerciseAttemptedUsersCount.ViewName}"" as 
SELECT a.""CourseId"", a.""SlideId"", COUNT(DISTINCT a.""UserId"") as ""AttemptedUsersCount""
FROM ""AutomaticExerciseCheckings"" AS a
GROUP BY (a.""CourseId"", a.""SlideId"");

CREATE UNIQUE INDEX ""IX_ExerciseAttemptedUsersCounts_Course_Slide""
ON public.""{ExerciseAttemptedUsersCount.ViewName}"" (""CourseId"", ""SlideId"");

CREATE MATERIALIZED VIEW public.""{ExerciseUsersWithRightAnswerCount.ViewName}"" as 
SELECT a.""CourseId"", a.""SlideId"", COUNT(DISTINCT a.""UserId"") as ""UsersWithRightAnswerCount""
FROM ""AutomaticExerciseCheckings"" AS a
WHERE a.""IsRightAnswer""
GROUP BY (a.""CourseId"", a.""SlideId"");

CREATE UNIQUE INDEX ""IX_ExerciseUsersWithRightAnswerCounts_Course_Slide""
ON public.""{ExerciseUsersWithRightAnswerCount.ViewName}"" (""CourseId"", ""SlideId"");
";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			var sql = @$"
DROP MATERIALIZED VIEW  public.""{ExerciseAttemptedUsersCount.ViewName}"";
DROP MATERIALIZED VIEW  public.""{ExerciseUsersWithRightAnswerCount.ViewName}"";
";
			migrationBuilder.Sql(sql);
		}
    }
}
