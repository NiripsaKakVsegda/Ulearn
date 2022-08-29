using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Courses.Manager;

namespace Database;

public class WebCourseManager : CourseManager, IWebCourseManager
{
	private static ILog log => LogProvider.Get().ForContext(typeof(WebCourseManager));
	
	private readonly IServiceScopeFactory serviceScopeFactory;

	public WebCourseManager(
		IServiceScopeFactory serviceScopeFactory
	)
	{
		this.serviceScopeFactory = serviceScopeFactory;
	}

	public new DirectoryInfo GetExtractedCourseDirectory(string courseId)
	{
		return CourseManager.GetExtractedCourseDirectory(courseId);
	}

	public async Task UpdateCoursesAsync()
	{
		using (var scope = serviceScopeFactory.CreateScope())
		{
			var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
			var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersions();
			foreach (var publishedVersion in publishedCourseVersions)
			{
				var courseId = publishedVersion.CourseId;
				var publishedVersionId = publishedVersion.Id;
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedVersionId), TimeSpan.FromSeconds(0.1));
			}
		}
	}

	public async Task UpdateTempCoursesAsync()
	{
		using (var scope = serviceScopeFactory.CreateScope())
		{
			var tempCoursesRepo = scope.ServiceProvider.GetService<ITempCoursesRepo>();
			var recentTempCourses = await tempCoursesRepo.GetRecentTempCourses();
			foreach (var tempCourse in recentTempCourses)
			{
				var courseId = tempCourse.CourseId;
				var publishedLoadingTime = tempCourse.LoadingTime;
				await UpdateCourseOrTempCourseToVersionFromDirectory(courseId, new CourseVersionToken(publishedLoadingTime), TimeSpan.FromSeconds(0.1));
			}
		}
	}

	public async Task<bool> CreateCourseIfNotExists(string courseId, Guid versionId, string courseTitle, string userId)
	{
		using (var scope = serviceScopeFactory.CreateScope())
		{
			var coursesRepo = scope.ServiceProvider.GetService<ICoursesRepo>();
			var hasCourse = await coursesRepo.GetPublishedCourseVersion(courseId) != null;
			if (!hasCourse)
			{
				var helpVersionFile = await coursesRepo.GetPublishedVersionFile(ExampleCourseId);
				using (var exampleCourseZip = SaveVersionZipToTemporaryDirectory(courseId, new CourseVersionToken(versionId), new MemoryStream(helpVersionFile.File)))
				{
					CourseZipWithTitleUpdater.Update(exampleCourseZip.FileInfo, courseTitle);
					await coursesRepo.AddCourseVersion(courseId, courseTitle, versionId, userId, null, null, null, null, await exampleCourseZip.FileInfo.ReadAllContentAsync()
						.ConfigureAwait(false)).ConfigureAwait(false);
				}

				await coursesRepo.MarkCourseVersionAsPublished(versionId).ConfigureAwait(false);
			}

			return !hasCourse;
		}
	}

	public async Task<byte[]> GetTempCourseZipBytes(string courseId)
	{
		var path = GetExtractedCourseDirectory(courseId);
		using (await CourseLock.AcquireReaderLockAsync(courseId).ConfigureAwait(false))
		{
			using var stream = ZipUtils.CreateZipFromDirectory(new List<string> { path.FullName }, new List<string> { CourseVersionToken.VersionFileName }, null);
			return stream.ToArray();
		}
	}
}