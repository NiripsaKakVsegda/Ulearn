using System;
using System.IO;
using System.Threading;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCheckerJob
{
	public static class SandboxRunHelper
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(SandboxRunHelper));

		public static RunningResults WithSubmissionWorkingDirectory(RunnerSubmission submission, ISandboxRunnerClient runnerClient,
			DirectoryInfo workingDirectory, bool deleteSubmissionsAfterFinish)
		{
			if (!workingDirectory.Exists)
			{
				try
				{
					workingDirectory.Create();
				}
				catch (Exception e)
				{
					log.Error(e, $"Не могу создать директорию для решений: {workingDirectory}");
					return new RunningResults(submission.Id, Verdict.SandboxError, error: e.ToString());
				}
			}

			var randomSuffix = Guid.NewGuid().ToString("D");
			randomSuffix = randomSuffix.Substring(randomSuffix.Length - 8);
			var submissionWorkingDirectory = workingDirectory.GetSubdirectory($"{submission.Id}-{randomSuffix}");
			try
			{
				submissionWorkingDirectory.Create();
			}
			catch (Exception e)
			{
				log.Error(e, $"Не могу создать директорию для решения: {submissionWorkingDirectory.FullName}");
				return new RunningResults(submission.Id, Verdict.SandboxError, error: e.ToString());
			}

			try
			{
				return runnerClient.RunContainerAndGetResultInternal(submission, submissionWorkingDirectory);
			}
			catch (Exception ex)
			{
				log.Error(ex, "RunContainerAndGetResultInternal error");
				return new RunningResults(submission.Id, Verdict.SandboxError, error: ex.ToString());
			}
			finally
			{
				if (deleteSubmissionsAfterFinish)
				{
					log.Info($"Удаляю папку с решением: {submissionWorkingDirectory}");
					SafeRemoveDirectory(submissionWorkingDirectory.FullName);
				}
			}
		}

		private static void SafeRemoveDirectory(string path)
		{
			try
			{
				/* Sometimes we can't remove directory after Time Limit Exceeded, because process is alive yet. Just wait some seconds before directory removing */
				FuncUtils.TrySeveralTimes(() =>
				{
					Directory.Delete(path, true);
					return true;
				}, 3, () => Thread.Sleep(TimeSpan.FromSeconds(1)));
			}
			catch (Exception e)
			{
				log.Warn(e, $"Произошла ошибка при удалении директории {path}, но я её проигнорирую");
			}
		}
	}
}