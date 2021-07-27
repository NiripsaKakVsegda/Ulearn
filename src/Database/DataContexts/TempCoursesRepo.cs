using System;
using System.Collections.Generic;
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
			return db.TempCourses.ToList();
		}

		// Временные курсы, которые обновлялись недавно. Только такие будем поднимать в память
		public List<TempCourse> GetRecentTempCourses()
		{
			var monthAgo = DateTime.Now.Subtract(TimeSpan.FromDays(30));
			return db.TempCourses.Where(tc => tc.LoadingTime > monthAgo).ToList();
		}
	}
}