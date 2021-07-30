using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses;

namespace Database.Repos
{
	public interface ITempCoursesRepo
	{
		Task<TempCourse> Find(string courseId);
		Task<List<TempCourse>> GetAllTempCourses();
		Task<List<TempCourse>> GetRecentTempCourses();
		Task<TempCourseError> GetCourseError(string courseId);
		Task<TempCourse> AddTempCourse(string courseId, string authorId, CourseVersionToken versionToke);
		Task<DateTime> UpdateTempCourseLoadingTime(string courseId, CourseVersionToken versionToke);
		Task<TempCourseError> UpdateOrAddTempCourseError(string courseId, string error);
		Task MarkTempCourseAsNotErrored(string courseId);
	}
}