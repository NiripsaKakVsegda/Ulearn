using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;
using Ulearn.Common;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("build-containers", HelpText = "build docker containers for checking solutions of exercises in sanbox")]
	public class BuildDockerContainerOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Console.WriteLine("You need to run sandboxes/build.sh on non-Windows");
				return;
			}

			var dockerExistsOnPath = EnvironmentVariablesUtils.ExistsOnPath("docker.exe");
			if (!dockerExistsOnPath)
			{
				Console.WriteLine("Docker not found in PATH");
				return;
			}

			var pathToBuildBat = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(BuildDockerContainerOptions)).Location), "sandboxes/build.bat");
			var process = new Process
			{
				StartInfo =
				{
					Arguments = "",
					FileName = pathToBuildBat,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					UseShellExecute = false,
					WorkingDirectory = Path.GetDirectoryName(pathToBuildBat)
				}
			};
			process.Start();
			process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
			process.BeginOutputReadLine();

			process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
			process.BeginErrorReadLine();
			process.WaitForExit();
			process.Close();
		}
	}
}