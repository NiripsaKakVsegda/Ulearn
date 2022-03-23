namespace Ulearn.Core.Courses.Slides
{
	public interface ISlideLoader
	{
		Slide Load(SlideLoadingContext context);
	}
}