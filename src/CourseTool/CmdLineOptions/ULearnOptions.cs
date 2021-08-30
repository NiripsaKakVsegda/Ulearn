using System;
using CommandLine;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("ulearn", HelpText = "Operations with uLearn")]
	class ULearnOptions : AbstractOptions
	{
		[Option("download", HelpText = "Download course package")]
		public bool Download { get; set; }

		[Option("upload", HelpText = "Upload course package")]
		public bool Upload { get; set; }

		[Option('f', "force", HelpText = "Try force")]
		public bool Force { get; set; }

		public override void DoExecute()
		{
			if (!(Download ^ Upload))
			{
				Console.Out.WriteLine("Use one of options: download or upload");
				return;
			}

			Console.Out.WriteLine("Profile: {0}", Profile);
			var profile = Config.GetProfile(Profile);
			var credentials = Credentials.GetCredentials(WorkingDirectory, Profile, "uLearn");

			if (Download)
				ULearnInteractor.Download(WorkingDirectory, Force, Config, profile.UlearnBaseUrlWeb, credentials);
			if (Upload)
				ULearnInteractor.Upload(WorkingDirectory, Config, profile.UlearnBaseUrlWeb, credentials);
		}
	}
}