namespace uLearn.Web.Core.Models;

public class HintPageModel
{
	public HintWithLikeButton[] Hints { get; set; }
}

public class HintWithLikeButton
{
	public int HintId { get; set; }
	public bool IsLiked { get; set; }
	public string CourseId { get; set; }
	public Guid SlideId { get; set; }
	public string Hint { get; set; }
}