using System.IO;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;

namespace Ulearn.Web.Api.Utils
{
	public interface IMasterCourseManager : ICourseUpdater
	{
		DirectoryInfo GetExtractedCourseDirectory(string courseId);
		Task<FileInfo> GenerateOrFindStudentZip(string courseId, Slide slide);
		Task<TempCourse> CreateTempCourse(string baseCourseId, string tmpCourseId, string userId);
		string GetTmpCourseId(string baseCourseId, string userId);
		Task<(TempCourse Course, string Error)> UpdateTempCourseFromStream(string tmpCourseId, Stream updateZipContent, bool isFullCourse);
	}
}