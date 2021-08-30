using CommandLine;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-pack-targz", HelpText = "Create <CourseName>.tar.gz from olx directory")]
	public class OlxPackTarGzOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			EdxInteraction.CreateEdxCourseArchive(WorkingDirectory, Config.ULearnCourseId, true);
		}
	}

	[Verb("olx-pack-tar", HelpText = "Create <CourseName>.tar from olx directory")]
	public class OlxPackTarOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			EdxInteraction.CreateEdxCourseArchive(WorkingDirectory, Config.ULearnCourseId, false);
		}
	}
}