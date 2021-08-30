using System;
using System.IO;
using CommandLine;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-desquash-chapters", HelpText = "Move sequentials from chapter")]
	public class OlxDesquashChaptersOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			Console.WriteLine("desquashing");
			new OlxSquasher().DesquashCourse(Path.Combine(WorkingDirectory, "olx"));
			Console.WriteLine("Now you can create targz and upload course to edx");
		}
	}
}