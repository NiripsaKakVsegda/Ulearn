using System;
using System.Threading.Tasks;
using Ulearn.Core.Courses.Manager;

namespace Database
{
	public interface IWebCourseManager : ISlaveCourseManager
	{
		Task<bool> CreateCourseIfNotExists(string courseId, Guid versionId, string courseTitle, string userId);
	}
}