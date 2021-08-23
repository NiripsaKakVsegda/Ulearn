using System.Collections.Generic;
using System.IO;
using Database.Models;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Markdown;
using Ulearn.Web.Api.Models.Responses.Exercise;

namespace Ulearn.Web.Api.Controllers.Slides
{
	public class ExerciseSlideRendererContext
	{
		public ExerciseAttemptsStatistics AttemptsStatistics;
		public bool CanSeeCheckerLogs;
		public MarkdownRenderContext markdownRenderContext;
	}
}