using System;
using System.Diagnostics;
using System.IO;

namespace GitCourseUpdater
{
	public static class GitBashCmdWorker
	{
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

			var cmd = new Process();

			cmd.StartInfo = new ProcessStartInfo
			{
				FileName = "cmd.exe",
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				RedirectStandardError = true,
				CreateNoWindow = false,
				UseShellExecute = false,
			};
			cmd.OutputDataReceived += (process, output) => Console.WriteLine(output.Data);
			cmd.Start();
			cmd.BeginOutputReadLine();

			var launchBash = "\"%PROGRAMFILES%\\Git\\bin\\sh.exe\"";

			var bashCommands =
				// get into repo directory
				$"cd \'{repoDir}\'" +
				//start ssh-agent for ssh keys holding, also prints process PID and keeps it
				$" && eval $(ssh-agent -s)" +
				//adding git to known hosts
				$"ssh-keyscan -t rsa github.com >> ~/.ssh/known_hosts" +
				//add identity to ssh-agent
				$" && ssh-add \'{pathToPEMKey}\' " +
				//clone repo
				$" && git {gitOperation} {gitRepoUrl} {gitOperationAdditionalFlags}" +
				//stop ssh-agent using PID generated before
				$" && eval $(ssh-agent -k)" +
				//exiting sh.exe
				$" && exit";

			cmd.StandardInput.WriteLine($"{launchBash} -c \"{bashCommands}\"");

			//exiting cmd.exe
			cmd.StandardInput.WriteLine("exit");

			cmd.WaitForExit();

			if (cmd.ExitCode != 0)
				throw new Exception(cmd.StandardError.ReadToEnd());
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