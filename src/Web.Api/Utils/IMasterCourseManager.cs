using System;
using System.IO;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;

namespace Ulearn.Web.Api.Utils
{
	public interface IMasterCourseManager : ICourseUpdater
	{
		FileInfo GetStagingCourseFile(string courseId);
		FileInfo GetStagingTempCourseFile(string courseId);
		DirectoryInfo GetExtractedCourseDirectory(string courseId);
		void ReloadCourseNotSafe(string courseId, bool notifyAboutErrors = true);
		void ExtractTempCourseChanges(string tempCourseId);
		Task<TempCourse> CreateTempCourse(string baseCourseId, string tmpCourseId, string userId);
		Task<FileInfo> GenerateOrFindStudentZip(string courseId, Slide slide);
	}
}