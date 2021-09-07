using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using Microsoft.AspNet.Identity;
using Ulearn.Core.Courses.Manager;
using Vostok.Logging.Abstractions;

namespace Database.Repos
{
	public class GoogleSheetExportTasksRepo
	{
		private readonly ULearnDb db;
		private static ILog log => LogProvider.Get().ForContext(typeof(GoogleSheetExportTasksRepo));
		private readonly GroupsRepo groupsRepo;
		private readonly ICourseStorage courseStorage;

		public GoogleSheetExportTasksRepo()
			: this(new ULearnDb(), WebCourseManager.CourseStorageInstance)
		{
		}

		public GoogleSheetExportTasksRepo(ULearnDb db, ICourseStorage courseStorage)
		{
			this.db = db;
			this.courseStorage = courseStorage;
			groupsRepo = new GroupsRepo(db, courseStorage);
		}

		public List<GoogleSheetExportTask> GetVisibleGoogleSheetTask(string courseId, List<Group> groups, IPrincipal user)
		{
			if (groups == null)
				return null;
			var userId = user.Identity.GetUserId();
			var groupsIds = groups.Select(g => g.Id);

			var accessibleAsMemberGroupsIds = db.GroupMembers.Where(a => a.UserId == userId)
				.Select(g => g.GroupId).ToHashSet();
			var accessibleCourseGroupsIds = user.HasAccessFor(courseId, CourseRole.Instructor)
				? groupsRepo.GetAvailableForUserGroups(courseId, user).Select(g => g.Id)
				: new int[0];

			accessibleAsMemberGroupsIds.UnionWith(accessibleCourseGroupsIds);
			accessibleAsMemberGroupsIds.IntersectWith(groupsIds);
			if (accessibleAsMemberGroupsIds.Count == 0)
				return null;

			var tasksIds = db.GoogleSheetExportTaskGroups
				.Where(g => accessibleAsMemberGroupsIds.Contains(g.GroupId))
				.Select(g => g.TaskId).ToHashSet();

			var currentUtcTime = DateTime.UtcNow;
			return db.GoogleSheetExportTasks
				.Include(t => t.Author)
				.Include(t => t.Groups.Select(g => g.Group))
				.Where(t => tasksIds.Contains(t.Id))
				.Where(t => t.RefreshStartDate <= currentUtcTime)
				.GroupBy(t => t.Author.Id)
				.OrderBy(t => t.Key == userId)
				.SelectMany(t => t)
				.OrderBy(t => t.RefreshEndDate)
				.ToList();
		}
	}
}