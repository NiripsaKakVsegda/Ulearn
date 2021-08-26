using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-remove-unused-files", HelpText = "Removes unused files from directories: vertical, html, static, video, lti")]
	public class OlxRemoveUnusedFilesOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			var olxDirectory = Path.Combine(WorkingDirectory, "olx");

			var directoriesToClean = new[] { "vertical", "html", "static", "video", "lti" };
			var allFiles = GetAllFiles(directoriesToClean, olxDirectory);

			var directoryToUsedXmls = new Dictionary<string, HashSet<FileInEdxCourse>>();
			var loadOptions = new EdxLoadOptions
			{
				OnLoadExistingEdxItem = fic =>
				{
					var directory = fic.Directory;
					if (directoryToUsedXmls.ContainsKey(directory))
						directoryToUsedXmls[directory].Add(fic);
					else
						directoryToUsedXmls[directory] = new HashSet<FileInEdxCourse> { fic };
				}
			};

			Console.WriteLine("Loading");
			var course = EdxCourse.Load(olxDirectory, loadOptions);

			RemoveUnusedNonStaticFiles(directoryToUsedXmls, allFiles, olxDirectory);

			RemoveUnusedStaticFiles(olxDirectory, allFiles);

			Console.WriteLine("Saving");
			course.Save(olxDirectory);
		}

		private static Dictionary<string, HashSet<FileInEdxCourse>> GetAllFiles(string[] directoriesToClean, string olxDirectory)
		{
			var allFiles = new Dictionary<string, HashSet<FileInEdxCourse>>();
			foreach (var directory in directoriesToClean)
			{
				var files = Directory.GetFiles(Path.Combine(olxDirectory, directory)).Select(fn => new FileInfo(fn)).ToList();
				foreach (var file in files)
				{
					var fic = new FileInEdxCourse(directory, Path.GetFileNameWithoutExtension(file.Name), file.Extension.Trim('.'));
					if (allFiles.ContainsKey(directory))
						allFiles[directory].Add(fic);
					else
						allFiles[directory] = new HashSet<FileInEdxCourse> { fic };
				}
			}
			return allFiles;
		}

		private static void RemoveUnusedNonStaticFiles(Dictionary<string, HashSet<FileInEdxCourse>> directoryToUsedXmls, Dictionary<string, HashSet<FileInEdxCourse>> allFiles, string olxDirectory)
		{
			var xmlFilesToRemove = new List<FileInEdxCourse>();
			foreach (var directoryWithXmls in directoryToUsedXmls.Keys)
			{
				if (!allFiles.ContainsKey(directoryWithXmls))
					continue;
				var actualXmlFiles = allFiles[directoryWithXmls].Where(e => e.Extension == "xml").ToList();
				var xmlsToRemove = actualXmlFiles.ToHashSet();
				xmlsToRemove.ExceptWith(directoryToUsedXmls[directoryWithXmls]);
				xmlFilesToRemove.AddRange(xmlsToRemove);
			}

			foreach (var xmlToRemove in xmlFilesToRemove)
			{
				File.Delete(Path.Combine(olxDirectory, xmlToRemove.Directory, $"{xmlToRemove.FileName}.{xmlToRemove.Extension}"));
				if (xmlToRemove.Directory == "html")
				{
					var htmlFile = new FileInfo(Path.Combine(olxDirectory, xmlToRemove.Directory, $"{xmlToRemove.FileName}.html"));
					if (htmlFile.Exists)
						htmlFile.Delete();
				}
			}
		}

		private void RemoveUnusedStaticFiles(string olxDirectory, Dictionary<string, HashSet<FileInEdxCourse>> allFiles)
		{
			var contentOfAllHtmlFiles = GetContentOfAllHtmlFiles(olxDirectory);
			var staticFilesToRemove = new List<string>();
			foreach (var staticFile in allFiles["static"])
			{
				var fileName = $"{staticFile.FileName}.{staticFile.Extension}";
				if (!contentOfAllHtmlFiles.Contains(fileName))
					staticFilesToRemove.Add(fileName);
			}

			foreach (var staticFileToRemove in staticFilesToRemove)
			{
				File.Delete(Path.Combine(olxDirectory, "static", staticFileToRemove));
				var htmlFile = new FileInfo(Path.Combine(olxDirectory, "static", staticFileToRemove));
				if (htmlFile.Exists)
					htmlFile.Delete();
			}

			CopyStaticDirectoryFromCourseToolToOlx();
		}

		private static string GetContentOfAllHtmlFiles(string olxDirectory)
		{
			var allHtmlFiles = Directory.GetFiles(olxDirectory, "*.html", SearchOption.AllDirectories);
			var sb = new StringBuilder();
			foreach (var htmlFile in allHtmlFiles)
			{
				var content = File.ReadAllText(htmlFile);
				sb.Append(content);
			}
			return sb.ToString();
		}
	}
}