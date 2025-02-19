﻿using System;
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

			//TODO (rozentor) do not know why, but dev determines path to build tools of 2017 which are deleted, in that case we need to override it with 2022
			if (path == assembly || path.Contains("TestRunner") || path.Contains("2017"))
			{
				// TODO использовать PowerShell module to locate MSBuild: vssetup.powershell. Get-VSSetupInstance
				var newBuildTools = @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin";
				var newBuildToolsX86 = @"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin";
				var buildToolsMsBuildDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin";
				var vsCommunityMsBuildDirectory = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";
				if (Directory.Exists(newBuildToolsX86))
				{
					//otherwise it will look for net6 analog, which doesn't exist
					Environment.SetEnvironmentVariable("MicrosoftNETBuildExtensionsTasksAssembly",
						Path.Combine(@"C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Microsoft\Microsoft.NET.Build.Extensions\tools\net472", 
							"Microsoft.NET.Build.Extensions.Tasks.dll"));
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(newBuildToolsX86, "MSBuild.exe"));
				}
				else if (Directory.Exists(newBuildTools))
				{
					//otherwise it will look for net6 analog, which doesn't exist
					Environment.SetEnvironmentVariable("MicrosoftNETBuildExtensionsTasksAssembly",
						Path.Combine(@"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Microsoft\Microsoft.NET.Build.Extensions\tools\net472", 
							"Microsoft.NET.Build.Extensions.Tasks.dll"));
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(newBuildTools, "MSBuild.exe"));
				}
				else if (Directory.Exists(buildToolsMsBuildDirectory))
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(buildToolsMsBuildDirectory, "MSBuild.exe"));
				else if (Directory.Exists(vsCommunityMsBuildDirectory))
					Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(vsCommunityMsBuildDirectory, "MSBuild.exe"));
			}
			pathToMsBuild = ToolLocationHelper.GetPathToBuildTools(version);
			log.Info("PathToMsBuild {PathToMsBuild}", pathToMsBuild);
		}
	}
}