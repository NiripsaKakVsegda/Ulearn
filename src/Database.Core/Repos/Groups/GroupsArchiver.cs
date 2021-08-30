using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos.Groups
{
	public interface IGroupsArchiver
	{
		Task<List<int>> GetOldGroupsToArchive(string courseId = null);
	}

	public class GroupsArchiver : IGroupsArchiver
	{
		private readonly UlearnDb db;

		public GroupsArchiver(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<int>> GetOldGroupsToArchive(string courseId = null)
		{
			var yearAgo = DateTime.Today.AddYears(-1);
			var groupsQueryable = db.Groups
				.Where(g => !g.IsArchived && !g.IsDeleted && (g.CreateTime == null || g.CreateTime < yearAgo));
			if (courseId != null)
				groupsQueryable = groupsQueryable.Where(g => g.CourseId == courseId);
			var groupsIds = await groupsQueryable.Select(g => g.Id).ToListAsync();
			if (groupsIds.Count == 0)
				return new List<int>();
			var groupsIdsWithNewUsers = (await db.GroupMembers.Where(m => groupsIds.Contains(m.GroupId) && m.AddingTime > yearAgo)
					.Select(g => g.GroupId)
					.ToListAsync())
				.ToHashSet();
			var groupsIdsWithNewAccesses = (await db.GroupAccesses.Where(a => groupsIds.Contains(a.GroupId) && a.GrantTime > yearAgo)
					.Select(g => g.GroupId)
					.ToListAsync())
				.ToHashSet();
			return groupsIds.Where(g => !groupsIdsWithNewUsers.Contains(g) && !groupsIdsWithNewAccesses.Contains(g)).ToList();
		}
	}
}