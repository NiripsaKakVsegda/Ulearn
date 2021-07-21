﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models;
using Ulearn.Web.Api.Models.Parameters;
using Ulearn.Web.Api.Models.Responses;
using Ulearn.Web.Api.Models.Responses.Courses;
using Ulearn.Web.Api.Utils;


namespace Ulearn.Web.Api.Controllers
{
	[Route("/course-score-sheet/export/to-google-sheets")]
	public class GoogleSheetsStatisticController : BaseController
	{
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly IUnitsRepo unitsRepo;
		private readonly IGroupsRepo groupsRepo;
		private readonly ControllerUtils controllerUtils;
		private readonly IVisitsRepo visitsRepo;
		private readonly IAdditionalScoresRepo additionalScoresRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IGoogleSheetExportTasksRepo googleSheetExportTasksRepo;
		private readonly UlearnConfiguration configuration;
		
		public GoogleSheetsStatisticController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IGroupMembersRepo groupMembersRepo,
			IUnitsRepo unitsRepo, IGroupsRepo groupsRepo, ControllerUtils controllerUtils, IVisitsRepo visitsRepo,
			IAdditionalScoresRepo additionalScoresRepo, IGroupAccessesRepo groupAccessesRepo, IOptions<UlearnConfiguration> configuration,
			IGoogleSheetExportTasksRepo googleSheetExportTasksRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.courseRolesRepo = courseRolesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.unitsRepo = unitsRepo;
			this.groupsRepo = groupsRepo;
			this.controllerUtils = controllerUtils;
			this.visitsRepo = visitsRepo;
			this.additionalScoresRepo = additionalScoresRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.configuration = configuration.Value;
			this.googleSheetExportTasksRepo = googleSheetExportTasksRepo;
		}
		
		[HttpGet("tasks")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult<GoogleSheetsExportTaskListResponse>> GetAllCourseTasks([FromQuery] string courseId)
		{
			var exportTasks = await googleSheetExportTasksRepo.GetTasks(courseId);
			var responses = exportTasks
				.Select(exportTask => new GoogleSheetsExportTaskResponse
					{
						Id = exportTask.Id,
						AuthorInfo = BuildShortUserInfo(exportTask.Author),
						Groups = exportTask.Groups.Select(e => BuildShortGroupInfo(e.Group)).ToList(),
						IsVisibleForStudents = exportTask.IsVisibleForStudents,
						RefreshStartDate = exportTask.RefreshStartDate,
						RefreshEndDate = exportTask.RefreshEndDate,
						RefreshTimeInMinutes = exportTask.RefreshTimeInMinutes,
						SpreadsheetId = exportTask.SpreadsheetId,
						ListId = exportTask.ListId
					})
				.ToList();
			return new GoogleSheetsExportTaskListResponse
			{
				GoogleSheetsExportTasks = responses
			};
		}
		
		[HttpGet("tasks/{taskId}")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult<GoogleSheetsExportTaskResponse>> GetOneTask([FromRoute] int taskId)
		{
			throw new NotImplementedException();
		}
		
		[HttpPost("tasks")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult<GoogleSheetsExportTaskResponse>> AddNewTask([FromBody] GoogleSheetsExportTaskParams param)
		{
			var exportTask = await googleSheetExportTasksRepo.AddTask(param.CourseId, UserId, param.IsVisibleForStudents,
				param.RefreshStartDate, param.RefreshEndDate, param.RefreshTimeInMinutes, param.GroupsIds,
				param.SpreadsheetId, param.ListId);

			var result = new GoogleSheetsExportTaskResponse
			{
				Id = exportTask.Id,
				AuthorInfo = BuildShortUserInfo(exportTask.Author),
				Groups = exportTask.Groups.Select(e => BuildShortGroupInfo(e.Group)).ToList(),
				IsVisibleForStudents = exportTask.IsVisibleForStudents,
				RefreshStartDate = exportTask.RefreshStartDate,
				RefreshEndDate = exportTask.RefreshEndDate,
				RefreshTimeInMinutes = exportTask.RefreshTimeInMinutes,
				SpreadsheetId = exportTask.SpreadsheetId,
				ListId = exportTask.ListId
			};
			return result;
		}
		
		[HttpPatch("tasks/{taskId}")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult> UpdateTask([FromBody] GoogleSheetsExportTaskUpdateParams param, [FromRoute] int taskId)
		{
			var task = googleSheetExportTasksRepo.GetTaskById(taskId);
			if (task == null)
				return NotFound();
			// добавить проверки на доступ везде
			await googleSheetExportTasksRepo.UpdateTask(taskId,
				param.IsVisibleForStudents, param.RefreshStartDate,
				param.RefreshEndDate, param.RefreshTimeInMinutes);
			return Ok();
		}
		
		[HttpDelete("tasks/{taskId}")]
		[Authorize(Policy = "Instructors")]
		public async Task<ActionResult> DeleteTask([FromRoute] int taskId)
		{
			await googleSheetExportTasksRepo.DeleteTask(taskId);
			return NoContent();
		}
	}
}