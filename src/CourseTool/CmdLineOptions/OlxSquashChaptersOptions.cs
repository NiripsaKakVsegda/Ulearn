using System;
using System.IO;
using CommandLine;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-squash-chapters", HelpText = "Move sequentials to chapters")]
	public class OlxSquashChaptersOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			Console.WriteLine("squashing");
			new OlxSquasher().SquashCourse(Path.Combine(WorkingDirectory, "olx"));
			Console.WriteLine("Now you can edit all sequentials (subchapters) in olx/chapters folder... After editing run olx-desquash-chapters command");
		}
	}
}