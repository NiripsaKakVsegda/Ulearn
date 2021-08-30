using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Core.Courses
{
	public static class InstructorNoteLoader
	{
		public static Slide Load(SlideLoadingContext context)
		{
			var markdown = context.SlideFile.ContentAsUtf8();
			var slide = new Slide(new MarkdownBlock(markdown) { Hide = true })
			{
				Id = context.Unit.Id,
				Title = "Заметки преподавателю",
				Hide = true
			};
			slide.BuildUp(context);
			slide.Validate(context);
			return slide;
		}
	}
}