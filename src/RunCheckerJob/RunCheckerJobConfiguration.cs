using Ulearn.Core.Configuration;

namespace RunCheckerJob
{
	public class RunCheckerJobConfiguration : UlearnConfigurationBase
	{
		public RunCheckerJobConfigurationContent RunCheckerJob { get; set; }
	}

	public class RunCheckerJobConfigurationContent
	{
		public double? SleepSeconds { get; set; }
		public int? ThreadsCount { get; set; }
		public string AgentName { get; set; } // By default agentName is computer name
		public string SubmissionsWorkingDirectory { get; set; } // Directory for compiling msbuild solutions. By default it's ./submissions in current directory
		public bool? DeleteSubmissions { get; set; } // Delete submissions after checking
	}
}