using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses;

namespace Database.Repos
{
	public interface IUnitsRepo
	{
		Task<List<Guid>> GetVisibleUnitIds(Course course, string userId);
		Task<List<Guid>> GetPublishedUnitIds(Course course);
		Task<List<UnitAppearance>> GetUnitAppearances(Course course);
		Task<bool> IsUnitVisibleForStudents(Course course, Guid unitId);
		Task<DateTime?> GetNextUnitPublishTime(string courseId);
		Task<HashSet<string>> GetVisibleCourses();
		Task<bool> IsCourseVisibleForStudents(string courseId);
	}
}