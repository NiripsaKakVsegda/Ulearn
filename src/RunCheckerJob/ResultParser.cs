using System;
using Vostok.Logging.Abstractions;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCheckerJob
{
	public static class ResultParser
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(ResultParser));

		public static RunningResults Parse(string stdout, string stderr, InterpretNonJsonOutputType interpretNonJsonOutputAs)
		{
			try
			{
				var result = JsonConvert.DeserializeObject<RunningResults>(stdout);
				if (result == null)
					throw new Exception();
				return result;
			}
			catch (Exception)
			{
				log.Warn("Не удалось распарсить результат");
				return interpretNonJsonOutputAs switch
				{
					InterpretNonJsonOutputType.CompilationError => new RunningResults(Verdict.CompilationError) { CompilationOutput = stderr },
					InterpretNonJsonOutputType.WrongAnswer => new RunningResults(Verdict.WrongAnswer) { Output = stderr },
					InterpretNonJsonOutputType.SandboxError => new RunningResults(Verdict.SandboxError) { Logs = new[] { "Не удалось распарсить результат", "Exit code: 0", $"stdout: {stdout}", $"stderr: {stderr}" } },
					_ => new RunningResults(Verdict.SandboxError) { Logs = new[] { "Не удалось распарсить результат", "Exit code: 0", $"stdout: {stdout}", $"stderr: {stderr}" } }
				};
			}
		}
	}
}