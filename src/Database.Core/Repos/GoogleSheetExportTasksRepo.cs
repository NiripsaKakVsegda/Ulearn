using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Groups;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;

namespace Database.Repos
{
	public class GoogleSheetExportTasksRepo : IGoogleSheetExportTasksRepo
	{
		private readonly UlearnDb db;
		private static ILog log => LogProvider.Get().ForContext(typeof(GoogleSheetExportTasksRepo));
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly ICourseRolesRepo courseRolesRepo;

		public GoogleSheetExportTasksRepo(UlearnDb db, IGroupAccessesRepo groupAccessesRepo, ICourseRolesRepo courseRolesRepo)
		{
			this.db = db;
			this.groupAccessesRepo = groupAccessesRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		public async Task<int> AddTask(string courseId, string authorId,
			bool isVisibleForStudents, DateTime? refreshStartDate,
			DateTime? refreshEndDate, int? refreshTimeInMinutes,
			List<int> groupsIds, string spreadsheetId, int listId)
		{
			var exportTaskGroups = new List<GoogleSheetExportTaskGroup>();
			foreach (var groupId in groupsIds)
			{
				var exportTaskGroup = new GoogleSheetExportTaskGroup
				{
					GroupId = groupId
				};
				exportTaskGroups.Add(exportTaskGroup);
				db.GoogleSheetExportTaskGroups.Add(exportTaskGroup);
			}

			var exportTask = new GoogleSheetExportTask
			{
				CourseId = courseId,
				AuthorId = authorId,
				Groups = exportTaskGroups,
				IsVisibleForStudents = isVisibleForStudents,
				RefreshStartDate = refreshStartDate,
				RefreshEndDate = refreshEndDate,
				RefreshTimeInMinutes = refreshTimeInMinutes,
				SpreadsheetId = spreadsheetId,
				ListId = listId,
			};
			db.GoogleSheetExportTasks.Add(exportTask);
			await db.SaveChangesAsync();
			return exportTask.Id;
		}

		public async Task<GoogleSheetExportTask> GetTaskById(int taskId)
		{
			return await db.GoogleSheetExportTasks
				.Include(t => t.Author)
				.Include(t => t.Groups).ThenInclude(g => g.Group)
				.Where(t => t.Id == taskId)
				.FirstOrDefaultAsync();
		}

		public async Task<List<GoogleSheetExportTask>> GetTasks(string courseId, string authorId = null)
		{
			return await db.GoogleSheetExportTasks
				.Include(t => t.Author)
				.Include(t => t.Groups).ThenInclude(g => g.Group)
				.Where(t => t.CourseId == courseId)
				.Where(t => authorId == null || t.AuthorId == authorId)
				.ToListAsync();
		}

		public async Task<List<GoogleSheetExportTask>> GetAllTasks()
		{
			return await db.GoogleSheetExportTasks
				.Include(t => t.Author)
				.Include(t => t.Groups).ThenInclude(g => g.Group)
				.ToListAsync();
		}

		public async Task UpdateTask(GoogleSheetExportTask exportTask, bool isVisibleForStudents, DateTime? refreshStartDate,
			DateTime? refreshEndDate, int? refreshTimeInMinutes, string spreadsheetId, int listId)
		{
			exportTask.IsVisibleForStudents = isVisibleForStudents;
			exportTask.RefreshStartDate = refreshStartDate;
			exportTask.RefreshEndDate = refreshEndDate;
			exportTask.RefreshTimeInMinutes = refreshTimeInMinutes;
			exportTask.SpreadsheetId = spreadsheetId;
			exportTask.ListId = listId;
			await db.SaveChangesAsync();
		}

		public async Task SaveTaskUploadResult(GoogleSheetExportTask exportTask, DateTime lastUpdateTime, string error = null)
		{
			exportTask.LastUpdateDate = lastUpdateTime;
			exportTask.LastUpdateErrorMessage = error;
			await db.SaveChangesAsync();
		}

		public async Task DeleteTask(GoogleSheetExportTask exportTask)
		{
			db.GoogleSheetExportTasks.Remove(exportTask);
			await db.SaveChangesAsync();
		}

		public async Task<List<GoogleSheetExportTask>> GetVisibleGoogleSheetTask(string courseId, List<SingleGroup> groups, string userId)
		{
			if (groups == null)
				return null;
			var isInstructor = await courseRolesRepo.HasUserAccessToCourse(userId, courseId, CourseRoleType.Instructor).ConfigureAwait(false);
			var groupsIds = groups.Select(g => g.Id);

			var accessibleAsMemberGroupsIds = db.GroupMembers
				.Where(a => a.UserId == userId)
				.Select(g => g.GroupId)
				.ToHashSet();
			var accessibleCourseGroupsIds = (await groupAccessesRepo
					.GetAvailableForUserGroupsAsync(courseId, userId, true, true, true, GroupQueryType.SingleGroup))
				.Select(g => g.Id);

			accessibleAsMemberGroupsIds.UnionWith(accessibleCourseGroupsIds);
			accessibleAsMemberGroupsIds.IntersectWith(groupsIds);
			if (accessibleAsMemberGroupsIds.Count == 0)
				return null;

			var tasksIds = db.GoogleSheetExportTaskGroups
				.Where(g => accessibleAsMemberGroupsIds.Contains(g.GroupId))
				.Select(g => g.TaskId)
				.ToHashSet();

			var currentUtcTime = DateTime.UtcNow;
			var query = db.GoogleSheetExportTasks
				.Include(t => t.Author)
				.Include(t => t.Groups)
				.ThenInclude(g => g.Group)
				.Where(t => tasksIds.Contains(t.Id) && t.RefreshStartDate <= currentUtcTime);

			if (!isInstructor)
				query = query.Where(t => t.IsVisibleForStudents);

			return await query
				.OrderBy(t => t.RefreshEndDate)
				.ToListAsync();
			/*
			 * .GroupBy(t => t.Author.Id)
				.OrderBy(t => t.Key == userId)
				.SelectMany(t => t)
			 */
		}
	}
}