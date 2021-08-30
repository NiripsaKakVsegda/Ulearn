using System;
using System.IO;
using Ulearn.Core.Configuration;

namespace RunCheckerJob
{
	public class SandboxRunnerSettings
	{
		public TimeSpan MaintenanceTimeLimit = TimeSpan.FromSeconds(10);
		public TimeSpan TestingTimeLimit = TimeSpan.FromSeconds(10);
		public int MemoryLimit = 64 * 1024 * 1024;
		public int OutputLimit = 10 * 1024 * 1024;
		public TimeSpan WaitSandboxAfterKilling = TimeSpan.FromSeconds(5);

		public static readonly DirectoryInfo WorkingDirectory;
		public static readonly bool DeleteSubmissionsAfterFinish;

		static SandboxRunnerSettings()
		{
			var configuration = ApplicationConfiguration.Read<RunCheckerJobConfiguration>().RunCheckerJob;
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			WorkingDirectory = new DirectoryInfo(Path.Combine(baseDirectory, "submissions"));
			var deleteSubmissions = configuration.DeleteSubmissions ?? true;
			DeleteSubmissionsAfterFinish = deleteSubmissions;
			var workingDirectory = configuration.SubmissionsWorkingDirectory;
			if (!string.IsNullOrWhiteSpace(workingDirectory))
				WorkingDirectory = new DirectoryInfo(workingDirectory);
		}
	}
}