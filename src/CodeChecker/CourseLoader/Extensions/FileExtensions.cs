using System.Runtime.InteropServices;
using CourseLoader.Helpers;

namespace CourseLoader.Extensions
{
	public static class FileExtensions
	{
		public static IEnumerable<FileInfo> GetFilesByMask(this DirectoryInfo directory, string mask)
		{
			if (mask.Contains(".."))
				throw new ArgumentException($"{nameof(mask)} can not contain \"..\"", nameof(mask));

			/* Replace slashes / to backslashes \ on Windows */
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				mask = mask.Replace('/', '\\');

			return GlobSearcher.Glob(Path.Combine(directory.FullName, mask)).Select(path => new FileInfo(path));
		}
	}
}