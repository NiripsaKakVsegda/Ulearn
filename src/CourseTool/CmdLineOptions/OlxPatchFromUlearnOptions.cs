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
	[Verb("olx-patch-from-ulearn", HelpText = "Patch Edx course with new slides from uLearn course")]
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
				ulearnCourse.GetSlidesNotSafe()
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
			if (Config.EmitSequentialsForInstructorNotes)
				PatchInstructorsNotes(edxCourse, ulearnCourse, patcher.OlxPath);
		}

		private void PatchInstructorsNotes(EdxCourse edxCourse, Course ulearnCourse, string olxPath)
		{
			var ulearnUnits = ulearnCourse.GetUnitsNotSafe();
			foreach (var chapter in edxCourse.CourseWithChapters.Chapters)
			{
				var chapterUnit = ulearnCourse.GetUnitsNotSafe().FirstOrDefault(u => u.Title == chapter.DisplayName);
				var chapterNote = chapterUnit?.InstructorNote;
				if (chapterUnit == null || chapterNote == null)
					continue;

				var unitIndex = ulearnUnits.IndexOf(chapterUnit);
				var displayName = "Заметки преподавателю";
				var sequentialId = $"{ulearnCourse.Id}-{unitIndex}-note-seq";
				var verticalId = $"{ulearnCourse.Id}-{unitIndex}-note-vert";
				var mdBlockId = $"{ulearnCourse.Id}-{unitIndex}-note-md";
				var sequentialNote = new Sequential(sequentialId, displayName,
					new[]
					{
						new Vertical(verticalId, displayName,
							new[] { chapterNote.Blocks.OfType<MarkdownBlock>().First().ToEdxComponent(mdBlockId, displayName, CourseDirectory.FullName, chapterUnit.UnitDirectoryRelativeToCourse) })
					}) { VisibleToStaffOnly = true };
				if (!File.Exists($"{olxPath}/sequential/{sequentialNote.UrlName}.xml"))
				{
					var sequentials = chapter.Sequentials.ToList();
					sequentials.Add(sequentialNote);
					new Chapter(chapter.UrlName, chapter.DisplayName, chapter.Start, sequentials.ToArray()).Save(olxPath);
				}

				sequentialNote.Save(olxPath);
			}
		}
	}
}