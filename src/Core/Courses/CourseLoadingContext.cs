using System.IO;

namespace Ulearn.Core.Courses
{
	public class CourseLoadingContext
	{
		public string CourseId { get; }

		public CourseSettings CourseSettings { get; }

		public DirectoryInfo CourseDirectory { get; }

		public CourseLoadingContext(string courseId, CourseSettings courseSettings, DirectoryInfo courseDirectory)
		{
			CourseId = courseId;
			CourseSettings = courseSettings;
			CourseDirectory = courseDirectory;
		}
	}
}