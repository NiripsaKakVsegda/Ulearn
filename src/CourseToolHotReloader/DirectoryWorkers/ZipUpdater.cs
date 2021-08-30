using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CourseToolHotReloader.Dtos;
using Ulearn.Common;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public static class ZipUpdater
	{
		// Могут посылаться лишние пути на удаление, пдходящие под excludeCriterias. Сервер их проигнорирует, т.к. таких файлов нет на сервере.
		public static MemoryStream CreateZipByUpdates(string pathToFolder, IList<ICourseUpdate> courseUpdates, IList<ICourseUpdate> deletedFiles, List<string> excludeCriterias)
		{
			var deletedFileContentStr = string.Join("\r\n", deletedFiles.Select(u => Path.GetRelativePath(pathToFolder, u.FullPath)));
			var deletedFileContent = new FileContent
			{
				Path = "deleted.txt",
				Data = Encoding.UTF8.GetBytes(deletedFileContentStr)
			};
			var filesToUpdateOrCreate = courseUpdates
				.Where(update => Directory.Exists(update.FullPath))
				.SelectMany(update => Directory.GetFiles(update.FullPath, "*.*", SearchOption.AllDirectories))
				.Concat(courseUpdates.Where(update => File.Exists(update.FullPath)).Select(u => u.FullPath))
				.Distinct()
				.Select(fullPath => new FileContent
				{
					Path = Path.GetRelativePath(pathToFolder, fullPath),
					Data = File.ReadAllBytes(fullPath)
				})
				.Append(deletedFileContent)
				.ToList();
			return ZipUtils.CreateZipFromDirectory(new List<string>(), excludeCriterias, filesToUpdateOrCreate);
		}

		public static MemoryStream CreateZipByFolder(string pathToFolder, List<string> excludeCriterias)
		{
			return ZipUtils.CreateZipFromDirectory(new List<string> {pathToFolder}, excludeCriterias, null);
		}
	}
}