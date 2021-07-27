using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ulearn.Core.Courses;
using Ulearn.Web.Api.Controllers;
using Ulearn.Web.Api.Models.Responses.TempCourses;
using System.Text;
using System.Threading;
using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Ulearn.Common;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Utils;

namespace Web.Api.Tests.Controllers.TempCourses
{
	[TestFixture]
	public class TempCoursesControllerTests : BaseControllerTests
	{
		private TempCourseController tempCourseController;
		private ICoursesRepo coursesRepo;
		private ITempCoursesRepo tempCoursesRepo;
		private ICourseRolesRepo courseRolesRepo;
		private DirectoryInfo testCourseDirectory;
		private IMasterCourseManager courseManager;
		private ICourseStorage courseStorage;
		private ICourseUpdater courseUpdater;
		private DirectoryInfo workingCourseDirectory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			SetupTestInfrastructureAsync(services => { services.AddScoped<TempCourseController>(); }).GetAwaiter().GetResult();
			tempCourseController = GetController<TempCourseController>();
			coursesRepo = serviceProvider.GetService<ICoursesRepo>();
			tempCoursesRepo = serviceProvider.GetService<ITempCoursesRepo>();
			courseRolesRepo = serviceProvider.GetService<ICourseRolesRepo>();
			courseManager = serviceProvider.GetService<IMasterCourseManager>();
			courseStorage = serviceProvider.GetService<ICourseStorage>();
			courseUpdater = serviceProvider.GetService<ICourseUpdater>();
			testCourseDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "TempCourses", "Help"));
			workingCourseDirectory = new DirectoryInfo(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkingCourse")));
		}

		[SetUp]
		public void SetUp()
		{
			if (Directory.Exists(workingCourseDirectory.FullName))
			{
				DeleteNotEmptyDirectory(workingCourseDirectory.FullName);
			}

			Directory.CreateDirectory(workingCourseDirectory.FullName);
			DirectoryCopy(testCourseDirectory.FullName, workingCourseDirectory.FullName, true);
		}

		[Test]
		public async Task Create_ShouldSucceed_WithValidRequest()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("create_response_success");
			var result = await tempCourseController.CreateCourse(baseCourse.Id);
			Assert.AreEqual(ErrorType.NoErrors, result.Value.ErrorType);
		}

		[Test]
		public async Task Create_ShouldUpdateDB_WithValidRequest()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("create_DB_success");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tempCourseEntity = await tempCoursesRepo.FindAsync(courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id));
			Assert.NotNull(tempCourseEntity);
		}

		[Test]
		public async Task Create_ShouldReturnNoErrors_WhenCourseAlreadyExists()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("create_response_conflict");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var result = await tempCourseController.CreateCourse(baseCourse.Id);
			Assert.AreEqual(ErrorType.NoErrors, result.Value.ErrorType);
		}

		[Test]
		public async Task Create_ShouldReturnForbidden_WhenUserAccessIsLowerThanCourseAdmin()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("create_response_forbidden");
			await courseRolesRepo.ToggleRole(baseCourse.Id, TestUsers.User.Id, CourseRoleType.CourseAdmin, TestUsers.Admin.Id, "Забрал доступ к курсу");
			await AuthenticateUserInControllerAsync(tempCourseController, TestUsers.User);
			await tempCourseController.CreateCourse(baseCourse.Id);
			var result = await tempCourseController.CreateCourse(baseCourse.Id);
			Assert.AreEqual(ErrorType.Forbidden, result.Value.ErrorType);
		}

		[Test]
		public async Task UploadFullCourse_ShouldSucceed_WhenCourseIsValid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_response_success");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(testCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			var uploadResult = await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(ErrorType.NoErrors, uploadResult.Value.ErrorType);
			var tempCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var courseTokenFromDisk = CourseVersionToken.Load(courseManager.GetExtractedCourseDirectory(tempCourseId));
			var courseTokenFromDb = new CourseVersionToken(uploadResult.Value.LastUploadTime);
			Assert.AreEqual(courseTokenFromDb, courseTokenFromDisk);
		}

		[Test]
		public async Task UploadFullCourse_ShouldReturnNotFound_WhenUserDoesNotHaveTempVersionOfCourse()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_response_notFound");
			await AuthenticateUserInControllerAsync(tempCourseController, TestUsers.User);
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(testCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			var uploadResult = await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(ErrorType.NotFound, uploadResult.Value.ErrorType);
		}

		[Test]
		public async Task UploadFullCourse_ShouldUpdateDB_WhenCourseIsValid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_DB_success");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var loadTimeBeforeUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			var loadTimeAfterUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			Assert.Less(loadTimeBeforeUpload, loadTimeAfterUpload);
		}

		[Test]
		public async Task UploadFullCourse_ShouldOverrideCourseDirectory_WhenCourseIsValid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_directoryState_success");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var pathToExcessFile = Path.Combine(workingCourseDirectory.FullName, "excess.txt");
			await File.WriteAllTextAsync(pathToExcessFile, "");
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			File.Delete(pathToExcessFile);
			fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			var diff = GetDirectoriesDiff(workingCourseDirectory.FullName, courseDirectory.FullName);
			Assert.IsEmpty(diff);
		}

		[Test]
		public async Task UploadFullCourse_ShouldReturnCourseError_WhenCourseIsInvalid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_response_courseError");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			BreakCourse();
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			var result = await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(result.Value.ErrorType, ErrorType.CourseError);
		}

		[Test]
		public async Task UploadFullCourse_ShouldNotUpdateDB_WhenCourseIsInvalid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_DB_courseError");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var loadTimeBeforeUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			BreakCourse();
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			var loadTimeAfterUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			Assert.AreEqual(loadTimeBeforeUpload, loadTimeAfterUpload);
		}

		[Test]
		public async Task UploadFullCourse_ShouldNotUpdateDirectory_WhenCourseIsInvalid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("upload_directoryState_courseError");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			var directoryContentBeforeUpload = GetDirectoryContent(courseDirectory.FullName);
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			BreakCourse();
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadFullCourse(baseCourse.Id, new List<IFormFile> { file });
			var directoryContentAfterUpload = GetDirectoryContent(courseDirectory.FullName);
			Assert.AreEqual(directoryContentAfterUpload, directoryContentBeforeUpload);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldResponseNoErrors_WhenCourseIsValid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_DB_success");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			var response = await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(ErrorType.NoErrors, response.Value.ErrorType);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldUpdateDB_WhenCourseIsValid()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_DB_update");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var loadTimeBeforeUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			Thread.Sleep(10);
			var result = await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(ErrorType.NoErrors, result.Value.ErrorType);
			var loadTimeAfterUpload = (await tempCoursesRepo.FindAsync(tmpCourseId)).LoadingTime;
			Assert.Less(loadTimeBeforeUpload, loadTimeAfterUpload);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldUpdateDirectoryPartially_WithNewFiles()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_directoryState_newFilesSuccess");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var courseContent = GetDirectoryContent(workingCourseDirectory.FullName).ToList();
			var secondUploadFiles = new List<string> { "\\U99_Presentation" };
			var firstUploadFiles = courseContent.Except(secondUploadFiles);
			var firstZip = AddToZip(firstUploadFiles);
			var secondZip = AddToZip(secondUploadFiles);
			var file = GetFormFileFromZip(firstZip);
			await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			file = GetFormFileFromZip(secondZip);
			await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			var diff = GetDirectoriesDiff(workingCourseDirectory.FullName, courseDirectory.FullName);
			Assert.IsEmpty(diff);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldUpdateDirectoryPartially_WithDeleteFiles()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_directoryState_deleteSuccess");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var pathToExcessFile = Path.Combine(workingCourseDirectory.FullName, "excess.txt");
			await File.WriteAllTextAsync(pathToExcessFile, "");
			await SendFullCourseWithPartiallyUpload(baseCourse.Id);
			File.Delete(pathToExcessFile);
			var zipWithDelete = GetZipWithDelete(new List<string> { "excess.txt" });
			var file = GetFormFileFromZip(zipWithDelete);
			await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			var diff = GetDirectoriesDiff(workingCourseDirectory.FullName, courseDirectory.FullName);
			Assert.IsEmpty(diff);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldUpdateDirectoryPartially_WithValidDeleteDirectories()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_directoryState_deleteDirsSuccess");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			var relativePathToFile = Path.Combine("newDir", "newDir2", "excess.txt");
			var pathToExcessFile = Path.Combine(workingCourseDirectory.FullName, relativePathToFile);
			Directory.CreateDirectory(Path.Combine(workingCourseDirectory.FullName, "newDir", "newDir2"));
			await File.WriteAllTextAsync(pathToExcessFile, "");
			await SendFullCourseWithPartiallyUpload(baseCourse.Id);
			DeleteNotEmptyDirectory(Path.Combine(workingCourseDirectory.FullName, "newDir"));
			var zipWithDelete = GetZipWithDelete(new List<string> { "newDir" });
			var file = GetFormFileFromZip(zipWithDelete);
			await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			var diff = GetDirectoriesDiff(workingCourseDirectory.FullName, courseDirectory.FullName);
			Assert.IsEmpty(diff);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldReturnCourseError_WithInvalidDeleteFiles()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_response_courseError");
			await tempCourseController.CreateCourse(baseCourse.Id);
			await SendFullCourseWithPartiallyUpload(baseCourse.Id);
			var filesToDelete = new List<string> { "Prelude.cs" };
			var zipWithDelete = GetZipWithDelete(filesToDelete);
			var file = GetFormFileFromZip(zipWithDelete);
			var response = tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			Assert.AreEqual(ErrorType.CourseError, response.Result.Value.ErrorType);
		}

		[Test]
		public async Task UploadCoursePartially_ShouldNotUpdateDirectory_WithInvalidDeleteFiles()
		{
			var baseCourse = await CreateAndConfigureBaseCourseForUser("partiallyUpload_directoryState_courseError");
			await tempCourseController.CreateCourse(baseCourse.Id);
			var tmpCourseId = courseManager.GetTempCourseId(baseCourse.Id, TestUsers.User.Id);
			await SendFullCourseWithPartiallyUpload(baseCourse.Id);
			var filesToDelete = new List<string> { "Prelude.cs" };
			var zipWithDelete = GetZipWithDelete(filesToDelete);
			var file = GetFormFileFromZip(zipWithDelete);
			await tempCourseController.UploadCourse(baseCourse.Id, new List<IFormFile> { file });
			var courseDirectory = courseManager.GetExtractedCourseDirectory(tmpCourseId);
			var diff = GetDirectoriesDiff(workingCourseDirectory.FullName, courseDirectory.FullName);
			Assert.IsEmpty(diff);
		}

		private ZipFile GetZipWithDelete(List<string> relativePaths)
		{
			File.WriteAllLines(Path.Combine(workingCourseDirectory.FullName, "deleted.txt"), relativePaths);
			return AddToZip(new List<string> { "deleted.txt" });
		}

		private ZipFile AddToZip(IEnumerable<string> relativePaths)
		{
			var file = new ZipFile(Encoding.UTF8);
			foreach (var relativePath in relativePaths)
			{
				var fullPath = Path.Combine(workingCourseDirectory.FullName, relativePath);
				if (Directory.Exists(fullPath))
				{
					file.AddDirectory(fullPath, relativePath);
				}

				if (File.Exists(fullPath))
				{
					file.AddFile(fullPath, Path.GetDirectoryName(relativePath));
				}
			}

			return file;
		}

		private async Task SendFullCourseWithPartiallyUpload(string baseCourseId)
		{
			var fullCourseZip = new ZipFile(Encoding.UTF8);
			fullCourseZip.AddDirectory(workingCourseDirectory.FullName);
			var file = GetFormFileFromZip(fullCourseZip);
			await tempCourseController.UploadCourse(baseCourseId, new List<IFormFile> { file });
		}

		private async Task<Course> CreateAndConfigureBaseCourseForUser(string baseCourseId)
		{
			using var ms = ZipUtils.CreateZipFromDirectory(new List<string> { testCourseDirectory.FullName }, null, null);
			var initialVersion = Guid.NewGuid();
			await courseManager.CreateCourseIfNotExists(baseCourseId, initialVersion, "Заголовок", TestUsers.User.Id);
			var versionId = Guid.NewGuid();
			await coursesRepo.AddCourseVersion(baseCourseId, versionId, TestUsers.Admin.Id, null, null, null, null, ms.ToArray());
			await coursesRepo.MarkCourseVersionAsPublished(versionId);
			await coursesRepo.DeleteCourseVersion(baseCourseId, initialVersion);
			await courseRolesRepo.ToggleRole(baseCourseId, TestUsers.User.Id, CourseRoleType.CourseAdmin, TestUsers.Admin.Id, "Создал временный курс");
			await courseUpdater.UpdateCourses();
			var course = courseStorage.GetCourse(baseCourseId);
			await AuthenticateUserInControllerAsync(tempCourseController, TestUsers.User);
			return course;
		}

		private void BreakCourse()
		{
			File.Delete(Path.Combine(workingCourseDirectory.FullName, "Prelude.cs"));
		}

		private static IEnumerable<string> GetDirectoriesDiff(string path1, string path2)
		{
			var firstDirFiles = GetDirectoryContent(path1).ToList();
			var secondDirFiles = GetDirectoryContent(path2).ToList();
			var diffs = firstDirFiles
				.Except(secondDirFiles)
				.Concat(secondDirFiles.Except(firstDirFiles))
				.Except(new List<string> { "course.xml" }) // после создания курса в папке курса на сервере создается course.xml
				.Except(new List<string> { "deleted.txt" })
				.Except(new List<string> { ".version" })
				.ToList();
			return diffs;
		}

		private static IEnumerable<string> GetDirectoryContent(string path)
		{
			return
				Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
					.Select(x => TrimPrefix(x, path));
		}

		private static string TrimPrefix(string text, string prefix)
		{
			return text.Substring(text.IndexOf(prefix) + prefix.Length + 1);
		}

		private static void DeleteNotEmptyDirectory(string dirPath)
		{
			var files = Directory.GetFiles(dirPath);
			var dirs = Directory.GetDirectories(dirPath);

			foreach (var file in files)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			foreach (var dir in dirs)
			{
				DeleteNotEmptyDirectory(dir);
			}

			Directory.Delete(dirPath, false);
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			var dir = new DirectoryInfo(sourceDirName);

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			var dirs = dir.GetDirectories();
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			if (copySubDirs)
			{
				foreach (var subdir in dirs)
				{
					var temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		private static IFormFile GetFormFileFromZip(ZipFile fullCourseZip)
		{
			var stream = new MemoryStream();
			fullCourseZip.Save(stream);
			stream.Position = 0;
			var name = Guid.NewGuid() + ".zip";
			return new FormFile(stream, 0, stream.Length, name, name);
		}
	}
}