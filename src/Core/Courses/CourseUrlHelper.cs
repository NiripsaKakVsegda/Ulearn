using System;
using System.Linq;

namespace Ulearn.Core.Courses
{
	public static class CourseUrlHelper
	{
		// Не учитывает возможность /../
		// В качестве baseUrlApi можно передавать HtmlBlock.BaseUrlApiPlaceholder, если это ссылка для HtmlBlock
		public static string GetAbsoluteUrlToFile(string baseUrlApi, string courseId, string unitPathRelativeToCourse, string filePathRelativeToUnit)
		{
			return GetUrlFromParts(baseUrlApi, $"courses/{courseId}/files", unitPathRelativeToCourse, filePathRelativeToUnit);
		}

		// Не учитывает возможность /../
		public static string GetAbsoluteUrlToStudentZip(string baseUrlApi, string courseId, Guid slideId, string studentZipName)
		{
			return GetUrlFromParts(baseUrlApi, $"slides/{courseId}/{slideId}/exercise/student-zip/{studentZipName}");
		}

		private static string GetUrlFromParts(params string[] parts)
		{
			var ps = parts
				.Where(p => p != null)
				.Select(p => p.Replace('\\', '/').Trim('/'))
				.Where(p => !string.IsNullOrEmpty(p))
				.ToArray();
			return string.Join("/", ps);
		}
	}
}