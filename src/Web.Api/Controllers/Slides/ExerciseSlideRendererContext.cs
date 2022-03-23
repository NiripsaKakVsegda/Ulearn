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