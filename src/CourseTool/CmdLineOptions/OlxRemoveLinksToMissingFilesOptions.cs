using System;
using System.IO;
using CommandLine;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-remove-links-to-missing-files", HelpText = "Removes links to missing files from the course")]
	public class OlxRemoveLinksToMissingFilesOptions : AbstractOptions
	{
		public override void DoExecute()
		{
			var loadOptions = new EdxLoadOptions();
			loadOptions.FailOnNonExistingItem = false;
			loadOptions.HandleNonExistentItemTypeName = (type, url) => Console.WriteLine($"Skipped non existent item type:{type} urlName:{url}");
			var folderName = Path.Combine(WorkingDirectory, "olx");
			Console.WriteLine("Loading");
			var course = EdxCourse.Load(folderName, loadOptions);
			Console.WriteLine("Saving");
			course.Save(folderName);
		}
	}
}