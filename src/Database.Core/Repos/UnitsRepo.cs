using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;

namespace Database.Repos
{
	public class UnitsRepo : IUnitsRepo
	{
		private readonly UlearnDb db;
		private readonly ICourseStorage courseStorage;
		private readonly ICourseRolesRepo courseRolesRepo;

		public UnitsRepo(UlearnDb db, ICourseStorage courseStorage, ICourseRolesRepo courseRolesRepo)
		{
			this.db = db;
			this.courseStorage = courseStorage;
			this.courseRolesRepo = courseRolesRepo;
		}

		public async Task<List<Guid>> GetVisibleUnitIds(Course course, string userId)
		{
			var canSeeEverything = await courseRolesRepo.HasUserAccessToCourse(userId, course.Id, CourseRoleType.Tester);
			if (canSeeEverything)
				return course.GetUnitsNotSafe().Select(u => u.Id).ToList();

			return await GetPublishedUnitIds(course);
		}

		public async Task<List<Guid>> GetPublishedUnitIds(Course course)
		{
			var visibleUnitsIds = new HashSet<Guid>(await db.UnitAppearances
				.Where(u => u.CourseId == course.Id && u.PublishTime <= DateTime.Now)
				.Select(u => u.UnitId)
				.ToListAsync());
			return course
				.GetUnitsNotSafe()
				.Where(u => visibleUnitsIds.Contains(u.Id) || u.Settings.IsExtraContent)
				.Select(u => u.Id)
				.ToList();
		}

		public async Task<bool> IsUnitVisibleForStudents(Course course, Guid unitId)
		{
			return (await GetPublishedUnitIds(course)).Contains(unitId);
		}

		public async Task<List<UnitAppearance>> GetUnitAppearances(Course course)
		{
			return await db.UnitAppearances
				.Where(u => u.CourseId == course.Id)
				.ToListAsync();
		}

		public async Task<HashSet<string>> GetVisibleCourses()
		{
			var appearances = (await db.UnitAppearances
					.Where(u => u.PublishTime <= DateTime.Now)
					.Select(u => new { u.CourseId, u.UnitId })
					.ToListAsync())
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

		public async Task<bool> IsCourseVisibleForStudents(string courseId)
		{
			if (courseStorage.FindCourse(courseId) == null)
				return false;
			var visibleUnitsIds = await db.UnitAppearances
				.Where(u => u.CourseId == courseId)
				.Where(u => u.PublishTime <= DateTime.Now)
				.Select(u => u.UnitId)
				.ToListAsync();
			var units = (courseStorage.GetCourse(courseId)).GetUnitsNotSafe().Select(u => u.Id).ToHashSet();
			units.IntersectWith(visibleUnitsIds);
			return units.Any();
		}

		public async Task<DateTime?> GetNextUnitPublishTime(string courseId)
		{
			return await db.UnitAppearances
				.Where(u => u.CourseId == courseId && u.PublishTime > DateTime.Now)
				.Select(u => (DateTime?)u.PublishTime)
				.OrderBy(t => t)
				.FirstOrDefaultAsync();
		}
	}
}