using System;
using System.Diagnostics;
using System.IO;
using Vostok.Logging.Abstractions;

namespace GitCourseUpdater
{
	public static class GitShWorker
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(GitRepo));

		public static void PerformOperation(
			string gitOperation,
			string gitOperationAdditionalFlags,
			string repoDir,
			string pathToPEMKey,
			string gitRepoUrl
		)
		{
			if (!Directory.Exists(repoDir))
			{
				Directory.CreateDirectory(repoDir);
			}

			var pathToProgramFiles = Environment.GetEnvironmentVariable("PROGRAMFILES");

			var shProcess = new Process();
			shProcess.StartInfo = new ProcessStartInfo
			{
				FileName = $"{pathToProgramFiles}\\Git\\bin\\sh.exe",
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				Verb = "runas",
			};
			shProcess.OutputDataReceived += (process, output) =>
			{
				if (output.Data != null)
					log.Info(output.Data);
			};

			var error = "";
			var isSSHagentStopped = false;
			var errorRegex = new System.Text.RegularExpressions.Regex("fatal|denied|error");
			shProcess.ErrorDataReceived += (process, output) =>
			{
				if (!isSSHagentStopped)
				{
					shProcess.StandardInput.WriteLine("eval $(ssh-agent -k)");
					isSSHagentStopped = true;
				}

				if (shProcess.HasExited) return;
				if(output.Data != null)
				{
					if (errorRegex.IsMatch(output.Data)) //sometimes warning get into here, we're redirecting them to log instead
						error += output.Data + "\n";
					else
						log.Info(output.Data);
				}
				//exitting process
				shProcess.StandardInput.WriteLine("exit");
			};

			shProcess.Start();
			shProcess.BeginOutputReadLine();
			shProcess.BeginErrorReadLine();

			var bashCommands =
				// get into repo directory
				$"cd \'{repoDir}\'" +
				//start ssh-agent for ssh keys holding, also prints process PID and keeps it
				$" && eval $(ssh-agent -s)" +
				//add identity to ssh-agent
				$" && ssh-add \'{pathToPEMKey}\' " +
				//clone repo without checkinh known_hosts with GIT_SSH_COMMAND hack
				$" && GIT_SSH_COMMAND=\"ssh -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no\" git {gitOperation} {gitRepoUrl} \'{gitOperationAdditionalFlags}\'" +
				// removing GIT_SSH_COMMAND hack just in case
				$" && GIT_SSH_COMMAND=\"\"" +
				//stop ssh-agent using PID generated before
				$" && eval $(ssh-agent -k)" +
				//exiting sh.exe
				$" && exit";

			shProcess.StandardInput.WriteLine($"{bashCommands}");

			shProcess.WaitForExit();
			shProcess.CancelOutputRead();
			shProcess.CancelErrorRead();

			if (shProcess.ExitCode != 0 || error.Length > 0)
				throw new Exception(error);
		}

		public static void Fetch(
			string refSpecs,
			string repoDir,
			string pathToPEMKey,
			string gitRepoUrl)
		{
			PerformOperation("fetch", refSpecs, repoDir, pathToPEMKey, gitRepoUrl);
		}

		public static void Clone(
			string repoDir,
			string pathToPEMKey,
			string gitRepoUrl)
		{
			PerformOperation("clone", ".", repoDir, pathToPEMKey, gitRepoUrl);
		}
	}
}