using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Database.Models;

namespace Database.DataContexts
{
	public class TempCoursesRepo
	{
		private readonly ULearnDb db;

		public TempCoursesRepo()
			: this(new ULearnDb())
		{
		}

		public TempCoursesRepo(ULearnDb db)
		{
			this.db = db;
		}

		public TempCourse Find(string courseId)
		{
			return db.TempCourses.Find(courseId);
		}

		public List<TempCourse> GetAllTempCourses()
		{
			return db.TempCourses.Include(t => t.Author).ToList();
		}

		// Временные курсы, которые обновлялись недавно. Только такие будем поднимать в память.
		public List<TempCourse> GetRecentTempCourses()
		{
			// Пока что загружаются все временные курсы.
			// Потому что связанные с курсом объекты могут всплыть в других запросах вроде дай мне все доступные группы.
			// И это может привести к некорректным данным или исключению.
			return GetAllTempCourses();
			// var monthAgo = DateTime.Now.Subtract(TimeSpan.FromDays(30));
			// return db.TempCourses.Where(tc => tc.LoadingTime > monthAgo).ToList();
		}
	}
}