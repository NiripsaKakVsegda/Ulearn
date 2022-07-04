using Database.Models;
using JetBrains.Annotations;

namespace uLearn.Web.Core.Models;

public class GroupsFilterViewModel
{
	public string CourseId { get; set; }

	public List<string> SelectedGroupsIds { get; set; }

	public List<Group> Groups { get; set; }

	public string InputControlName { get; set; } = "group";

	[CanBeNull] // Если это студент
	public Dictionary<int, List<string>> UsersIdsWithGroupsAccess { get; set; }
}

public class GroupsComparer : IComparer<(Group Group, List<string> Instructors)>
{
	private readonly string userId;

	public GroupsComparer(string userId)
	{
		this.userId = userId;
	}

	public int Compare((Group Group, List<string> Instructors) a, (Group Group, List<string> Instructors) b)
	{
		List<string> GetInstructors((Group Group, List<string> Instructors) t)
			=> Enumerable.Repeat(t.Group.OwnerId, 1).Concat(t.Instructors.EmptyIfNull()).ToList();

		var instructorsInA = GetInstructors(a);
		var isUserInA = instructorsInA.Contains(userId);
		var instructorsInB = GetInstructors(b);
		var isUserInB = instructorsInB.Contains(userId);

		switch (instructorsInA.Count)
		{
			case 1 when isUserInA && instructorsInB.Count == 1 && isUserInB:
				return string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal);
			case 1 when isUserInA:
				return -1;
		}

		if (instructorsInB.Count == 1 && isUserInB)
			return 1;

		return isUserInA switch
		{
			true when isUserInB => string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal),
			true => -1,
			_ => isUserInB ? 1 : string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal)
		};
	}
}