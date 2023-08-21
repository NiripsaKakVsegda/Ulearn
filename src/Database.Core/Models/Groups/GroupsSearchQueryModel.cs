namespace Database.Models;

public class GroupsSearchQueryModel
{
	public string CourseId { get; set; }
	public string InstructorId { get; set; }
	public string MemberId { get; set; }
	public string Query { get; set; }
	public bool IncludeArchived { get; set; } = false;
	public int Offset { get; set; } = 0;
	public int Count { get; set; } = 100;
}