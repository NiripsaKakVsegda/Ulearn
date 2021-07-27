﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.RunCheckerJobApi;
using Ulearn.Web.Api.Utils;
using Ulearn.Web.Api.Utils.Courses;
using Web.Api.Configuration;

namespace Ulearn.Web.Api.Controllers.Runner
{
	[ApiController]
	[Produces("application/json")]
	public class RunnerGetSubmissionsController : ControllerBase
	{
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly WebApiConfiguration configuration;
		private static ILog log => LogProvider.Get().ForContext(typeof(RunnerGetSubmissionsController));

		public RunnerGetSubmissionsController(IOptions<WebApiConfiguration> options, IServiceScopeFactory serviceScopeFactory)
		{
			configuration = options.Value;
			this.serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Взять на проверку решения задач
		/// </summary>
		[HttpPost("/runner/get-submissions")]
		public async Task<ActionResult<List<RunnerSubmission>>> GetSubmissions([FromQuery] string token, [FromQuery] string sandboxes, [FromQuery] string agent)
		{
			if (configuration.RunnerToken != token)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Invalid token"));

			var sandboxesImageNames = sandboxes.Split(',').ToList();

			var sw = Stopwatch.StartNew();
			while (true)
			{
				using (var scope = serviceScopeFactory.CreateScope())
				{
					var userSolutionsRepo = (IUserSolutionsRepo)scope.ServiceProvider.GetService(typeof(IUserSolutionsRepo));
					var submission = await userSolutionsRepo.GetUnhandledSubmission(agent, sandboxesImageNames);
					if (submission != null || sw.Elapsed > TimeSpan.FromSeconds(10))
					{
						if (submission != null)
							log.Info($"Отдаю на проверку решение: [{submission.Id}], агент {agent}, только сначала соберу их");
						else
							return new List<RunnerSubmission>();

						var courseStorage = scope.ServiceProvider.GetService<ICourseStorage>();
						var courseManager = scope.ServiceProvider.GetService<IMasterCourseManager>();
						var builtSubmissions = new List<RunnerSubmission> { await ToRunnerSubmission(submission, courseStorage, courseManager) };
						log.Info($"Собрал решения: [{submission.Id}], отдаю их агенту {agent}");
						return builtSubmissions;
					}
				}

				await Task.Delay(TimeSpan.FromMilliseconds(50));
				await UnhandledSubmissionsWaiter.WaitAnyUnhandledSubmissions(TimeSpan.FromSeconds(8));
			}
		}

		private async Task<RunnerSubmission> ToRunnerSubmission(UserExerciseSubmission submission,
			ICourseStorage courseStorage, IMasterCourseManager courseManager)
		{
			log.Info($"Собираю для отправки в RunCsJob решение {submission.Id}");
			var slide = courseStorage.FindCourse(submission.CourseId)?.FindSlideByIdNotSafe(submission.SlideId);

			if (slide is ExerciseSlide exerciseSlide)
			{
				using (await CourseLock.AcquireReaderLock(submission.CourseId))
				{
					var courseDictionary = courseManager.GetExtractedCourseDirectory(submission.CourseId).FullName;
					if (exerciseSlide is PolygonExerciseSlide)
						return ((PolygonExerciseBlock)exerciseSlide.Exercise).CreateSubmission(
							submission.Id.ToString(),
							submission.SolutionCode.Text,
							submission.Language,
							courseDictionary
						);

					return exerciseSlide.Exercise.CreateSubmission(
						submission.Id.ToString(),
						submission.SolutionCode.Text,
						courseDictionary
					);
				}
			}

			return new FileRunnerSubmission
			{
				Id = submission.Id.ToString(),
				Code = "// no slide anymore",
				Input = "",
				NeedRun = true
			};
		}
	}
}