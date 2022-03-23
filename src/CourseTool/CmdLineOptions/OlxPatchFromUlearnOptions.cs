using System;
using System.Linq;
using CommandLine;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-patch-from-ulearn", HelpText = "Patch Edx course from uLearn course")]
	class OlxPatchFromUlearnOptions : OlxAbstractPatchOptions
	{
		public override void Patch(OlxPatcher patcher, Config config, Profile profile, EdxCourse edxCourse)
		{
			Console.WriteLine("Loading Ulearn course from {0}", CourseDirectory.Name);
			var ulearnCourse = new CourseLoader().Load(CourseDirectory, Config.ULearnCourseId);
			Console.WriteLine("Patching");
			var guids = Guids?.Split(',').Select(Utils.GetNormalizedGuid).ToList();

			patcher.PatchVerticals(
				edxCourse,
				ulearnCourse.GetUnitsNotSafe().SelectMany(u => u.GetSlides(false))
					.Where(s => !config.IgnoredUlearnSlides.Select(Guid.Parse).Contains(s.Id))
					.Where(s => guids == null || guids.Contains(s.NormalizedGuid))
					.Select(s => s.ToVerticals(
						ulearnCourse.Id,
						profile.UlearnBaseUrlApi,
						profile.UlearnBaseUrlWeb,
						config.LtiId,
						CourseDirectory
					).ToArray()),
				guids != null || !SkipExistingGuids
			);

			CopyStaticDirectoryFromCourseToolToOlx();
		}
	}
}