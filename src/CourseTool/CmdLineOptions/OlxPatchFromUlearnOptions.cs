using System;
using System.IO;
using System.Linq;
using CommandLine;
using Newtonsoft.Json;
using uLearn.CourseTool.Json;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides.Blocks;
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
			var videoJson = string.Format("{0}/{1}", WorkingDirectory, config.Video);
			var video = File.Exists(videoJson)
				? JsonConvert.DeserializeObject<Video>(File.ReadAllText(videoJson))
				: new Video { Records = new Record[0] };
			var videoHistory = VideoHistory.UpdateHistory(WorkingDirectory, video);
			var videoGuids = videoHistory.Records
				.SelectMany(x => x.Data.Select(y => Tuple.Create(y.Id, x.Guid.GetNormalizedGuid())))
				.ToDictionary(x => x.Item1, x => x.Item2);

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
						videoGuids,
						config.LtiId,
						CourseDirectory
					).ToArray()),
				guids != null || !SkipExistingGuids
			);
		}
	}
}