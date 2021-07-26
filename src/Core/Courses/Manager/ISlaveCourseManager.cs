using System;
using System.IO;
using System.Threading.Tasks;
using Ulearn.Common;
using Ulearn.Core.Helpers;

namespace Ulearn.Core.Courses.Manager
{
	public interface ISlaveCourseManager : ICourseUpdater
	{
		Task<TempDirectory> ExtractCourseVersionToTemporaryDirectory(string courseId, CourseVersionToken versionToken, byte[] zipContent);
		(Course Course, Exception Exception) LoadCourseFromDirectory(string courseId, DirectoryInfo extractedCourseDirectory);
		TempFile SaveVersionZipToTemporaryDirectory(string courseId, CourseVersionToken versionToken, Stream stream);
		bool IsCourseIdAllowed(string courseId);
	}
}