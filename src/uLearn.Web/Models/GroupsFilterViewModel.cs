using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using JetBrains.Annotations;

namespace uLearn.Web.Models
{
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

			if (instructorsInA.Count == 1 && isUserInA && instructorsInB.Count == 1 && isUserInB)
				return string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal);

			if (instructorsInA.Count == 1 && isUserInA)
				return -1;

			if (instructorsInB.Count == 1 && isUserInB)
				return 1;

			if (isUserInA && isUserInB)
				return string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal);

			if (isUserInA)
				return -1;

			if (isUserInB)
				return 1;

			return string.Compare(a.Group.Name, b.Group.Name, StringComparison.Ordinal);
		}
	}
}