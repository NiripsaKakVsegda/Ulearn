using System;
using System.Linq;
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
				stdout = stdout.Trim();

				var objectStartCommaIndex = stdout.IndexOf('{');
				var objectEndCommaIndex = stdout.LastIndexOf('}');

				RunningResults result;

				var isCorrectPrefix = objectStartCommaIndex == 0;
				var isCorrectSuffix = objectEndCommaIndex == stdout.Length - 1;

				if (!isCorrectPrefix || !isCorrectSuffix)
				{
					var objectToParse = stdout.Substring(objectStartCommaIndex, objectEndCommaIndex - objectStartCommaIndex + 1);
					result = JsonConvert.DeserializeObject<RunningResults>(objectToParse);
					var additionalParsingInfo = "";
					switch (isCorrectPrefix)
					{
						case false when !isCorrectSuffix:
							additionalParsingInfo = "до и после";
							break;
						case false:
							additionalParsingInfo = "до";
							break;
						default:
						{
							if (!isCorrectSuffix)
							{
								additionalParsingInfo = "после";
							}

							break;
						}
					}
					
					
					result.Logs = new[]
					{
						$"При парсинге результата были замечены сторонние символы {additionalParsingInfo} JSON объекта",
						"Полный вывод:", stdout,
					};
					log.Warn("При парсинге результата были замечены сторонние символы");
				}
				else result = JsonConvert.DeserializeObject<RunningResults>(stdout);

				if (result == null)
					throw new Exception();
				return result;
			}
			catch (Exception)
			{
				log.Warn("Не удалось распарсить результат");
				return interpretNonJsonOutputAs switch
				{
					InterpretNonJsonOutputType.CompilationError => new RunningResults(Verdict.CompilationError) { CompilationOutput = string.Join("\n", new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s))) },
					InterpretNonJsonOutputType.WrongAnswer => new RunningResults(Verdict.WrongAnswer) { Output = stdout, Error = stderr },
					InterpretNonJsonOutputType.SandboxError => new RunningResults(Verdict.SandboxError) { Logs = new[] { "Не удалось распарсить результат", "Exit code: 0", $"stdout: {stdout}", $"stderr: {stderr}" } },
					_ => new RunningResults(Verdict.SandboxError) { Logs = new[] { "Не удалось распарсить результат", "Exit code: 0", $"stdout: {stdout}", $"stderr: {stderr}" } }
				};
			}
		}
	}
}