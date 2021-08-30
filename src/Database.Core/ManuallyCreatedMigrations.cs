using System.Collections.Generic;
using Database.Migrations;

namespace Database
{
	public static class ManuallyCreatedMigrations
	{
		// Это список создан, чтобы обратить внимание, что эти миграции просто так взять и удалить нельзя
		// Потому что их содержимое написано вручную, не сгенерировано автоматически
		private static List<string> migrations = new List<string>
		{
			nameof(AddDiffFromDatabase),
			nameof(AddIndexAutomaticExerciseCheckingCourseSlideIsAnswer)
		};
	}
}