using System;
using System.IO;
using System.Linq;

namespace Ulearn.Core.Courses
{
	public static class CourseUrlHelper
	{
		// filePathRelativeToUnit может содержать .. Еесли будут лишнее, съест часть префикса ссылки.
		// В качестве baseUrlApi можно передавать HtmlBlock.BaseUrlApiPlaceholder, если это ссылка для HtmlBlock
		public static string GetAbsoluteUrlToFile(string baseUrlApi, string courseId, string unitPathRelativeToCourse, string filePathRelativeToUnit)
		{
			return GetUrlFromParts(baseUrlApi, $"courses/{courseId}/files", unitPathRelativeToCourse, filePathRelativeToUnit);
		}

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
			var url = string.Join("/", ps);
			var uri = new Uri(url);
			return uri.AbsoluteUri; // Применяет .. внутри ссылки
		}
	}
}