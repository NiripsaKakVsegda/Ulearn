using System.Linq;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Markdown;

namespace uLearn.Web.Models
{
	public class InstructorNoteModel
	{
		public InstructorNoteModel(Slide slide, string noteEditUrl, MarkdownRenderContext markdownContext)
		{
			UnitTitle = slide.Unit.Title;
			NoteEditUrl = noteEditUrl;
			var markdown = slide.Blocks.OfType<MarkdownBlock>().First().Markdown;
			RenderedMarkdownRaw = markdown.RenderMarkdown(markdownContext);
		}

		public string UnitTitle;
		public string NoteEditUrl;
		public string RenderedMarkdownRaw;
	}
}