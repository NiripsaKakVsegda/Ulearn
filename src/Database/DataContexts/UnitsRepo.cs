using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Database.Extensions;
using Database.Models;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;

namespace Database.DataContexts
{
	public class UnitsRepo
	{
		private readonly ULearnDb db;
		private readonly ICourseStorage courseStorage;

		public UnitsRepo(ULearnDb db)
		{
			this.db = db;
			courseStorage = WebCourseManager.CourseStorageInstance;
		}

		public IEnumerable<Guid> GetVisibleUnitIds(Course course, IPrincipal user)
		{
			var canSeeEverything = user.HasAccessFor(course.Id, CourseRole.Tester);
			if (canSeeEverything)
				return course.GetUnitsNotSafe().Select(u => u.Id);
			return GetVisibleUnitIds(course);
		}

		public IEnumerable<Guid> GetVisibleUnitIds(Course course)
		{
			var visibleUnitsIds = new HashSet<Guid>(db.UnitAppearances
				.Where(u => u.CourseId == course.Id && u.PublishTime <= DateTime.Now)
				.Select(u => u.UnitId));
			return course.GetUnitsNotSafe().Select(u => u.Id).Where(g => visibleUnitsIds.Contains(g)).ToList();
		}

		public bool IsUnitVisibleForStudents(Course course, Guid unitId)
		{
			return db.UnitAppearances.Any(u => u.UnitId == unitId && u.CourseId == course.Id && u.PublishTime <= DateTime.Now);
		}

		public DateTime GetLowestPublishTimeOfUnpublishedUnit(string courseId)
		{
			return db.UnitAppearances.Where(u => u.CourseId == courseId && u.PublishTime > DateTime.Now)
				.Select(u => u.PublishTime)
				.Concat(new[] { DateTime.MaxValue }).Min();
		}

		public HashSet<string> GetVisibleCourses()
		{
			var appearances = db.UnitAppearances
				.Where(u => u.PublishTime <= DateTime.Now)
				.Select(u => new { u.CourseId, u.UnitId })
				.AsEnumerable()
				.GroupBy(p => p.CourseId)
				.Where(g => courseStorage.FindCourse(g.Key) != null)
				.Where(g =>
				{
					var units = courseStorage.GetCourse(g.Key).GetUnitsNotSafe().Select(u => u.Id).ToHashSet();
					units.IntersectWith(g.Select(p => p.UnitId));
					return units.Any();
				})
				.Select(g => g.Key);
			return new HashSet<string>(appearances, StringComparer.OrdinalIgnoreCase);
		}
	}
}