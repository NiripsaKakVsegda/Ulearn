using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;

namespace Database.Repos
{
	public class GoogleSheetExportTasksRepo : IGoogleSheetExportTasksRepo
	{
		private readonly UlearnDb db;
		private static ILog log => LogProvider.Get().ForContext(typeof(GoogleSheetExportTasksRepo));

		public GoogleSheetExportTasksRepo(UlearnDb db)
		{
			this.db = db;
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

		public async Task DeleteTask(GoogleSheetExportTask exportTask)
		{
			db.GoogleSheetExportTasks.Remove(exportTask); 
			await db.SaveChangesAsync();
		}
	}
}