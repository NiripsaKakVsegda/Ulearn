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
				var verdict = Verdict.SandboxError;
				switch (interpretNonJsonOutputAs)
				{
					case InterpretNonJsonOutputType.CompilationError:
						verdict = Verdict.CompilationError;
						break;
					case InterpretNonJsonOutputType.SandboxError:
						verdict = Verdict.SandboxError;
						break;
					case InterpretNonJsonOutputType.WrongAnswer:
						verdict = Verdict.WrongAnswer;
						break;
				}

				log.Warn("Не удалось распарсить результат");
				return new RunningResults(verdict)
				{
					Logs = new[]
					{
						"Не удалось распарсить результат",
						"Exit code: 0",
						$"stdout: {stdout}",
						$"stderr: {stderr}"
					}
				};
			}
		}
	}
}