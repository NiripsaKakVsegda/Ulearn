using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using uLearn.CourseTool.Validating;
using Ulearn.Core.Courses;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("test", HelpText = "Run tests on course")]
	class TestCourseOptions : AbstractOptions
	{
		[Option('s', "slide", HelpText = "SlideId to test only one specific slide")]
		public string SlideId { get; set; }

		public override void DoExecute()
		{
			var ulearnDir = CourseDirectory;
			Console.Write("Loading Ulearn course from {0} ... ", ulearnDir.Name);
			var sw = Stopwatch.StartNew();
			var course = new CourseLoader().Load(ulearnDir, Config.ULearnCourseId);
			Console.WriteLine(sw.ElapsedMilliseconds + " ms");
			var slides = course.GetSlidesNotSafe();
			if (SlideId != null)
			{
				slides = slides.Where(s => s.Id == Guid.Parse(SlideId)).ToList();
				Console.WriteLine("Only slide " + SlideId);
			}

			var validator = new CourseValidator(slides, ulearnDir.FullName);
			validator.InfoMessage += m => Write(ConsoleColor.Gray, m);
			var errors = new List<string>();
			validator.Error += m =>
			{
				Write(ConsoleColor.Red, m);
				errors.Add(m);
			};
			validator.Warning += m =>
			{
				Write(ConsoleColor.DarkYellow, m);
				errors.Add(m);
			};
			validator.ValidateSpelling(course, ulearnDir.Name);
			validator.ValidateExercises();
			validator.ValidateVideos();
			validator.ValidateFlashcardSlides();
			validator.ValidateSlidesXml();
			if (errors.Any())
			{
				Console.WriteLine("Done! There are errors:");
				foreach (var error in errors)
				{
					Write(ConsoleColor.Red, error, true);
				}

				File.WriteAllText(course.Id + "-validation-report.html", GenerateHtmlReport(course, errors));
			}
			else
				Console.WriteLine("OK! No errors found");

			Console.WriteLine("Press any key...");
			Console.WriteLine("Exit code " + Environment.ExitCode);
			Console.ReadLine();
			Environment.Exit(Environment.ExitCode);
		}

		private string GenerateHtmlReport(Course course, List<string> errors)
		{
			var html = new StringBuilder();
			html.AppendLine("<!DOCTYPE html>");
			html.AppendLine("<html>");
			html.AppendLine("	<head>");
			html.AppendLine("		<meta charset='utf-8'>");
			html.AppendLine($"		<title>Валидация курса {course.Id}</title>");
			html.AppendLine("	</head>");
			html.AppendLine("	<body>");
			html.AppendLine($"		<h1>{course.Id}</h1>");
			var items = errors.Select(e => $"		<pre class='error'>{e}</pre>");
			html.AppendLine(string.Join("\n", items));
			html.AppendLine("	</body>");
			html.AppendLine("</html>");
			return html.ToString();
		}

		private void Write(ConsoleColor color, string message, bool error = false)
		{
			var oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			try
			{
				if (error)
				{
					Console.Error.WriteLine(message);
					Environment.ExitCode = -1;
				}
				else
					Console.WriteLine(message);
			}
			finally
			{
				Console.ForegroundColor = oldColor;
			}
		}
	}
}