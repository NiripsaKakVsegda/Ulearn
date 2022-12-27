using Ulearn.Core.Courses.Slides.Exercises.Blocks;

namespace uLearn.Web.Core.Models;

public class CodeModel
{
	public AbstractExerciseBlock ExerciseBlock;
	public string CourseId;
	public BlockRenderContext Context;
	public Guid SlideId;
}