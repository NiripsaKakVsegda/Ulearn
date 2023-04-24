using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IGoogleSheetExportTasksRepo
	{
		Task<int> AddTask(string courseId, string authorId,
			bool isVisibleForStudents, DateTime? refreshStartDate,
			DateTime? refreshEndDate, int? refreshTimeInMinutes,
			List<int> groupsIds, string spreadsheetId, int listId);

		Task<GoogleSheetExportTask> GetTaskById(int taskId);

		Task<List<GoogleSheetExportTask>> GetTasks(string courseId, string authorId = null);

		Task<List<GoogleSheetExportTask>> GetAllTasks();

		Task UpdateTask(GoogleSheetExportTask exportTask, bool isVisibleForStudents, DateTime? refreshStartDate,
			DateTime? refreshEndDate, int? refreshTimeInMinutes, string spreadsheetId, int listId);

		Task DeleteTask(GoogleSheetExportTask exportTask);

		Task SaveTaskUploadResult(GoogleSheetExportTask exportTask, DateTime lastUpdateTime, string error = null);
		Task<List<GoogleSheetExportTask>> GetVisibleGoogleSheetTask(string courseId, List<SingleGroup> groups, string userId);
	}
}