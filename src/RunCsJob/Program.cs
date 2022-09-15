using System;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RunCheckerJob;
using Vostok.Logging.Abstractions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Logging;
using Ulearn.Core.RunCheckerJobApi;
using Vostok.Logging.File;

namespace RunCsJob
{
	public class Program : ProgramBase
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(Program));
		private const string serviceName = "runcsjob";
		private readonly CsSandboxRunnerClient csSandboxRunnerClient;

		public static void Main(string[] args)
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			var logsSubdirectory = configuration.GraphiteServiceName + (configuration.Environment == "staging" ? "-dev" : null);
			LoggerSetup.Setup(configuration.HostLog, logsSubdirectory);
			try
			{
				DirectoryInfo сompilerDirectory = null;
				if (args.Any(x => x.StartsWith("-p:")))
				{
					var path = args.FirstOrDefault(x => x.StartsWith("-p:"))?.Substring(3);
					if (path != null)
						сompilerDirectory = new DirectoryInfo(path);
				}

				var isSelfCheck = args.Contains("--selfcheck");

				var program = new Program(сompilerDirectory);
				if (isSelfCheck)
					program.SelfCheck();
				else
					program.Run();
			}
			finally
			{
				FileLog.FlushAll();
			}
		}

		private Program([CanBeNull] DirectoryInfo сompilerDirectory, ManualResetEvent externalShutdownEvent = null)
			: base(serviceName, externalShutdownEvent, true)
		{
			if (сompilerDirectory != null)
				CsSandboxRunnerSettings.MsBuildSettings.CompilerDirectory = сompilerDirectory;
			if (!CsSandboxRunnerSettings.MsBuildSettings.CompilerDirectory.Exists)
			{
				log.Error($"Не найдена папка с компиляторами: {сompilerDirectory}");
				Environment.Exit(1);
			}

			log.Info($"Путь до компиляторов: {сompilerDirectory}");
			csSandboxRunnerClient = new CsSandboxRunnerClient();
		}

		protected override ISandboxRunnerClient SandboxRunnerClient => csSandboxRunnerClient;

		private void SelfCheck()
		{
			var res = SandboxRunnerClient.Run(new FileRunnerSubmission
			{
				Id = Utils.NewNormalizedGuid(),
				NeedRun = true,
				Code = "class C { static void Main(){ System.Console.WriteLine(\"Привет мир!\");}}"
			});
			log.Info("SelfCheck result: {Result}", JsonConvert.SerializeObject(res));
		}
	}
}