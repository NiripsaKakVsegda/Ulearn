using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool
{
	public static class Converter
	{
		private static Sequential[] UnitToSequentials(Course course, Config config, List<Unit> units, int unitIndex, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, Dictionary<string, string> videoGuids, DirectoryInfo courseDirectory)
		{
			var result = new List<Sequential>();
			var unit = units[unitIndex];

			var notHiddenOrIgnoredSlides = unit.GetSlides(false)
				.Where(s => !config.IgnoredUlearnSlides.Select(Guid.Parse).Contains(s.Id))
				.ToList();
			if (notHiddenOrIgnoredSlides.Any())
			{
				var sequentialForNotHiddenSlides = new Sequential($"{course.Id}-{unitIndex}-{0}", unit.Title,
					notHiddenOrIgnoredSlides
						.SelectMany(y => y.ToVerticals(course.Id, ulearnBaseUrlApi, ulearnBaseUrlWeb, videoGuids, config.LtiId, courseDirectory))
						.ToArray());
				result.Add(sequentialForNotHiddenSlides);
			}

			var note = unit.InstructorNote;
			var hiddenSlides = unit.GetHiddenSlides();
			if ((note != null || hiddenSlides.Count > 0) && config.EmitSequentialsForInstructorNotes)
			{
				if (hiddenSlides.Count > 0)
				{
					result.Add(new Sequential($"{course.Id}-{unitIndex}-{0}", unit.Title,
							hiddenSlides
							.Where(s => !config.IgnoredUlearnSlides.Select(Guid.Parse).Contains(s.Id))
							.SelectMany(y => y.ToVerticals(course.Id, ulearnBaseUrlApi, ulearnBaseUrlWeb, videoGuids, config.LtiId, courseDirectory))
							.ToArray()) { VisibleToStaffOnly = true }
					);
				}
				if (note != null)
				{
					var displayName = "Заметки преподавателю";
					var sequentialId = $"{course.Id}-{unitIndex}-note-seq";
					var verticalId = $"{course.Id}-{unitIndex}-note-vert";
					var mdBlockId = $"{course.Id}-{unitIndex}-note-md";
					result.Add(new Sequential(sequentialId, displayName,
							new[]
							{
								new Vertical(
									verticalId,
									displayName,
									new[]
									{
										unit.InstructorNote.Blocks.OfType<MarkdownBlock>().First()
											.ToEdxComponent(mdBlockId, displayName, courseDirectory.FullName, unit.UnitDirectoryRelativeToCourse)
									})
							}) { VisibleToStaffOnly = true }
					);
				}
			}

			return result.ToArray();
		}

		private static Chapter[] CourseToChapters(Course course, Config config, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, Dictionary<string, string> videoGuids, DirectoryInfo courseDirectory)
		{
			var units = course.GetUnitsNotSafe();
			return Enumerable
				.Range(0, units.Count)
				.Select(idx => new Chapter(
					$"{course.Id}-{idx}",
					units[idx].Title,
					null,
					UnitToSequentials(course, config, units, idx, ulearnBaseUrlApi, ulearnBaseUrlWeb, videoGuids, courseDirectory)))
				.Where(c => c.Sequentials.Length > 0)
				.ToArray();
		}

		public static EdxCourse ToEdxCourse(Course course, Config config, string ulearnBaseUrlApi, string ulearnBaseUrlWeb,
			Dictionary<string, string> youtubeId2UlearnVideoIds, DirectoryInfo courseDirectory)
		{
			return new EdxCourse(
				course.Id,
				config.Organization,
				course.Title,
				new[] { "lti" },
				null,
				CourseToChapters(course, config, ulearnBaseUrlApi, ulearnBaseUrlWeb, youtubeId2UlearnVideoIds, courseDirectory));
		}
	}
}