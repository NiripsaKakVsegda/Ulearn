using System;
using System.IO;
using Microsoft.Build.Utilities;
using Vostok.Logging.Abstractions;

namespace Ulearn.Core.Helpers
{
	public static class MsBuildLocationHelper
	{
		private static string pathToMsBuild;
		private static ILog log => LogProvider.Get().ForContext(typeof(MsBuildLocationHelper));

		public static void InitPathToMsBuild()
		{
			if (pathToMsBuild != null)
				return;
			const string version = "Current";
			var path = ToolLocationHelper.GetPathToBuildTools(version);
			var assembly = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(ProjModifier)).Location);
			if (path == assembly)
			{
				// TODO использовать PowerShell module to locate MSBuild: vssetup.powershell. Get-VSSetupInstance
				var buildToolsMsBuildDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin";
				var vsCommunityMsBuildDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";
				if (Directory.Exists(buildToolsMsBuildDirectory))
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(buildToolsMsBuildDirectory, "MSBuild.exe"));
				else if (Directory.Exists(vsCommunityMsBuildDirectory))
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(vsCommunityMsBuildDirectory, "MSBuild.exe"));
			}
			pathToMsBuild = ToolLocationHelper.GetPathToBuildTools(version);
			log.Info("PathToMsBuild {PathToMsBuild}", pathToMsBuild);
		}
	}
}