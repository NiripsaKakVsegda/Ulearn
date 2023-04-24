using System.Collections.Generic;
using System.Linq;
using Database.Models;

namespace Database.Repos.Groups;

public static class GroupCollectionExtensions
{
	public static IEnumerable<SingleGroup> AsGroups(this IEnumerable<GroupBase> groups) => groups.Cast<SingleGroup>();
}