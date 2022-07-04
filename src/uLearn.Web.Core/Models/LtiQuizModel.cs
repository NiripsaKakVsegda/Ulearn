using Ulearn.Core.Courses.Slides.Quizzes;

namespace uLearn.Web.Core.Models;

public class LtiQuizModel
{
	public QuizSlide Slide { get; set; }
	public string CourseId { get; set; }
	public string UserId { get; set; }
}