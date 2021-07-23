using System;
using System.IO;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Helpers;
using Vostok.Logging.Abstractions;

namespace Ulearn.Web.Api.Utils
{
	public class MasterCourseManager : SlaveCourseManager, IMasterCourseManager
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(MasterCourseManager));

		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly ExerciseStudentZipsCache exerciseStudentZipsCache;

		public MasterCourseManager(IServiceScopeFactory serviceScopeFactory, ExerciseStudentZipsCache exerciseStudentZipsCache)
			: base(serviceScopeFactory)
		{
			this.serviceScopeFactory = serviceScopeFactory;
			this.exerciseStudentZipsCache = exerciseStudentZipsCache;

			CourseStorageInstance.CourseChangedEvent += courseId =>
			{
				exerciseStudentZipsCache.DeleteCourseZips(courseId);
				ExerciseCheckerZipsCache.DeleteCourseZips(courseId);
			};
		}

		// Невременные курсы не выкладываются на диск сразу, а публикуются в базу и UpdateCourses их обновляет на диске.
		public override async Task UpdateCourses()
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
				var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();
				foreach (var publishedCourseVersion in publishedCourseVersions)
				{
					await UpdateCourseToVersionInDirectory(publishedCourseVersion, coursesRepo);
				}
			}
		}

		private async Task UpdateCourseToVersionInDirectory(CourseVersion publishedCourseVersions, ICoursesRepo coursesRepo)
		{
			var courseId = publishedCourseVersions.CourseId;
			var publishedVersionToken = new CourseVersionToken(publishedCourseVersions.Id);
			if (brokenVersions.ContainsKey(publishedVersionToken))
				return;
			var courseInMemory = CourseStorageInstance.FindCourse(courseId);
			if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
				return;

			if (courseInMemory == null) // Проверяем, вдруг на диске актуальная версия
			{
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, publishedVersionToken);
				courseInMemory = CourseStorageInstance.FindCourse(courseId);
				if (courseInMemory != null && courseInMemory.CourseVersionToken == publishedVersionToken)
					return;
			}

			var courseFile = await coursesRepo.GetVersionFile(publishedCourseVersions.Id);
			await UpdateCourseInCommonDirectory(courseId, courseFile.File, publishedVersionToken);
		}

		private async Task UpdateCourseInCommonDirectory(string courseId, byte[] content, CourseVersionToken publishedVersionToken)
		{
			using (var courseDirectory = await ExtractCourseVersionToTemporaryDirectory(courseId, publishedVersionToken, content))
			{
				var (course, error) = LoadCourseFromDirectory(courseId, courseDirectory.DirectoryInfo);
				if (error != null)
				{
					brokenVersions.TryAdd(publishedVersionToken, true);
					var message = $"Не смог загрузить с диска в память курс {courseId} версии {publishedVersionToken}";
					log.Error(error, message);
					return;
				}

				using (await CourseLock.AcquireWriterLock(courseId))
				{
					try
					{
						MoveCourse(course, courseDirectory.DirectoryInfo, GetExtractedCourseDirectory(courseId));
						CourseStorageUpdaterInstance.AddOrUpdateCourse(course);
					}
					catch (Exception ex)
					{
						log.Error(ex, $"Не смог переместить курс {courseId} версия {publishedVersionToken} из временной папки в общую");
					}
				}
			}
		}

		// Временные курсы выкладываются на диск сразу контроллером, который их создает, здесь только загружаю курсы с диска
		// Это актуально при старте, а в дальнейшем должен находить курсы на диске уже актуальной версии
		public override async Task UpdateTempCourses()
		{
			await base.UpdateTempCourses();
		}

		public async Task<FileInfo> GenerateOrFindStudentZip(string courseId, Slide slide)
		{
			return await exerciseStudentZipsCache.GenerateOrFindZip(courseId, slide, GetExtractedCourseDirectory(courseId).FullName);
		}

		public async Task<TempCourse> CreateTempCourse(string baseCourseId, string tmpCourseId, string userId)
		{
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
				var courseRolesRepo = scope.ServiceProvider.GetService<ICourseRolesRepo>();
				var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();

				var tmpCourseDbData = await tempCoursesRepo.FindAsync(tmpCourseId);
				if (tmpCourseDbData != null)
				{
					log.Warn($"Временный курс {tmpCourseId} уже существует в базе");

					var course = CourseStorageInstance.FindCourse(tmpCourseId);
					if (course != null && course.CourseVersionToken.LoadingTime == tmpCourseDbData.LoadingTime)
					{
						log.Warn($"Временный курс {tmpCourseId} версии {course.CourseVersionToken} уже загружен в память");
						return tmpCourseDbData;
					}
				}

				var loadingTime = DateTime.Now;
				var versionToken = new CourseVersionToken(loadingTime);
				var baseCourseVersionFile = await coursesRepo.GetPublishedVersionFile(baseCourseId);
				await UpdateCourseInCommonDirectory(tmpCourseId, baseCourseVersionFile.File, versionToken);

				if (tmpCourseDbData == null)
				{
					tmpCourseDbData = await tempCoursesRepo.AddTempCourseAsync(tmpCourseId, userId, loadingTime);
					await courseRolesRepo.ToggleRole(tmpCourseId, userId, CourseRoleType.CourseAdmin, userId, "Создал временный курс");
					return tmpCourseDbData;
				}
				await tempCoursesRepo.UpdateTempCourseLoadingTimeAsync(tmpCourseId, loadingTime);
				return await tempCoursesRepo.FindAsync(tmpCourseId);
			}
		}
	}
}