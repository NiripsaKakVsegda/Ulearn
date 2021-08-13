using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool
{
	public static class Converter
	{
		private static Sequential[] UnitToSequentials(Course course, Config config, List<Unit> units, int unitIndex, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, DirectoryInfo courseDirectory)
		{
			var unit = units[unitIndex];
			var notHiddenOrIgnoredSlides = unit.GetSlides(false)
				.Where(s => !config.IgnoredUlearnSlides.Select(Guid.Parse).Contains(s.Id))
				.ToList();

			if (!notHiddenOrIgnoredSlides.Any())
				return Array.Empty<Sequential>();

			var sequentialForNotHiddenSlides = new Sequential($"{course.Id}-{unitIndex}-{0}", unit.Title,
				notHiddenOrIgnoredSlides
					.SelectMany(y => y.ToVerticals(course.Id, ulearnBaseUrlApi, ulearnBaseUrlWeb, config.LtiId, courseDirectory))
					.ToArray());
			return new [] { sequentialForNotHiddenSlides };
		}

		private static Chapter[] CourseToChapters(Course course, Config config, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, DirectoryInfo courseDirectory)
		{
			var units = course.GetUnitsNotSafe();
			return Enumerable
				.Range(0, units.Count)
				.Select(idx => new Chapter(
					$"{course.Id}-{idx}",
					units[idx].Title,
					null,
					UnitToSequentials(course, config, units, idx, ulearnBaseUrlApi, ulearnBaseUrlWeb, courseDirectory)))
				.Where(c => c.Sequentials.Length > 0)
				.ToArray();
		}

		public static EdxCourse ToEdxCourse(Course course, Config config, string ulearnBaseUrlApi, string ulearnBaseUrlWeb, DirectoryInfo courseDirectory)
		{
			return new EdxCourse(
				course.Id,
				config.Organization,
				course.Title,
				new[] { "lti" },
				null,
				CourseToChapters(course, config, ulearnBaseUrlApi, ulearnBaseUrlWeb, courseDirectory));
		}
	}
}