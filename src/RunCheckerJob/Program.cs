using System;
using System.IO;
using System.Threading;
using System.Linq;
using Ulearn.Core.Configuration;
using Ulearn.Core.Logging;
using Ulearn.Core.RunCheckerJobApi;
using Vostok.Logging.File;

namespace RunCheckerJob
{
	public class Program : ProgramBase
	{
		private readonly DockerSandboxRunner sandboxRunner;

		public static void Main(string[] args)
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			var logsSubdirectory = configuration.GraphiteServiceName + (configuration.Environment == "staging" ? "-dev" : null);
			LoggerSetup.Setup(configuration.HostLog, logsSubdirectory);
			try
			{
				var selfCheckIndex = args.FindIndex("--selfcheck");

				var program = new Program(configuration.GraphiteServiceName);
				var hasNextArg = args.Length > selfCheckIndex + 1;
				if (selfCheckIndex >= 0 && hasNextArg)
					program.SelfCheck(args[selfCheckIndex + 1]);
				else
					program.Run();
			}
			finally
			{
				FileLog.FlushAll();
			}
		}

		private Program(string serviceName, ManualResetEvent externalShutdownEvent = null)
			: base(serviceName, externalShutdownEvent)
		{
			sandboxRunner = new DockerSandboxRunner();
		}

		protected override ISandboxRunnerClient SandboxRunnerClient => sandboxRunner;

		private void SelfCheck(string sandboxesDirectoryPath)
		{
			var sandboxesDirectory = new DirectoryInfo(sandboxesDirectoryPath);
			foreach (var dir in sandboxesDirectory.GetDirectories())
			{
				var res = new SelfChecker(new DockerSandboxRunner())
					.SelfCheck(dir);
				Console.WriteLine($"Verdict is {res.Verdict} for {dir.Name}");
			}
		}
	}
}