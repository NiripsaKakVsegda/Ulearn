using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses.Manager;

namespace uLearn.CourseTool.CmdLineOptions
{
	public abstract class AbstractOptions
	{
		protected AbstractOptions()
		{
			config = new Lazy<Config>(() => new FileInfo(ConfigFile).DeserializeXml<Config>());
		}

		private string dir;
		private string profile;

		[Option('d', "dir", HelpText = "Working directory for the project")]
		public string WorkingDirectory
		{
			get => dir ?? Directory.GetCurrentDirectory();
			set => dir = value;
		}

		[Option('p', "profile", HelpText = "Profile used to work with Edx and uLearn")]
		public string Profile
		{
			get => profile ?? "default";
			set => profile = value;
		}

		public string ConfigFile => WorkingDirectory + "/config.xml";

		public Config Config => config.Value;
		private readonly Lazy<Config> config;

		public DirectoryInfo CourseDirectory => GetCourseXmlDirectory(new DirectoryInfo(Path.Combine(WorkingDirectory, Config.ULearnCoursePackageRoot)));

		private static DirectoryInfo GetCourseXmlDirectory(DirectoryInfo directory)
		{
			if (!directory.GetFile("course.xml").Exists)
			{
				var courseXmlDirectory = directory.GetFiles("course.xml", SearchOption.AllDirectories).FirstOrDefault()?.Directory;
				if (courseXmlDirectory != null)
					directory = courseXmlDirectory;
			}
			return directory;
		}

		public void InitializeDirectoryIfNotYet()
		{
			if (!Directory.Exists(WorkingDirectory))
				Directory.CreateDirectory(WorkingDirectory);
			var configTemplateFile = Path.Combine(Utils.GetRootDirectory(), "templates/config.xml");
			File.Copy(configTemplateFile, ConfigFile);
			Process.Start("notepad", ConfigFile);
			Console.WriteLine("Edit the config file {0} and run this option again.", ConfigFile);
		}

		public void Execute()
		{
			if (File.Exists(ConfigFile))
				DoExecute();
			else
				InitializeDirectoryIfNotYet();
		}

		protected void CopyStaticFilesToOlx()
		{
			var staticDirectory = Path.Combine(Utils.GetAppPath(), "static"); // Статическая директоря проекта CourseTool
			foreach(var file in Directory.GetFiles(staticDirectory))
				File.Copy(file, Path.Combine(WorkingDirectory, "olx", "static", Path.GetFileName(file)), true);
		}

		public abstract void DoExecute();
	}
}