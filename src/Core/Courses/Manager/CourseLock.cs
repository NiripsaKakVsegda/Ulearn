using System;
using System.IO;
using System.Threading.Tasks;
using Ulearn.Common.Extensions;
using Vostok.Logging.Abstractions;

namespace Ulearn.Core.Courses.Manager
{
	public class CourseLock : IDisposable
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(CourseLock));

		private static readonly TimeSpan waitBetweenLockTries = TimeSpan.FromSeconds(0.1);
		private static readonly TimeSpan lockLifeTime = TimeSpan.FromMinutes(3);
		private static readonly DirectoryInfo coursesDirectory = CourseManager.GetCoursesDirectory().GetSubdirectory(CourseManager.CoursesSubdirectory);

		public bool IsLocked { get; private set; }
		private string courseId;

		private CourseLock(string courseId)
		{
			this.courseId = courseId;
		}

		// TODO сейчас Writer и Reader локи не отличаются
		public static async Task<CourseLock> AcquireWriterLock(string courseId)
		{
			var courseLock = new CourseLock(courseId);
			await courseLock.Lock(int.MaxValue);
			return courseLock;
		}

		public static async Task<CourseLock> AcquireReaderLock(string courseId)
		{
			var courseLock = new CourseLock(courseId);
			await courseLock.Lock(int.MaxValue);
			return courseLock;
		}

		public static CourseLock TryLock(string courseId)
		{
			var courseLock = new CourseLock(courseId);
			courseLock.Lock(1).RunSynchronously();
			return courseLock;
		}

		private async Task Lock(int attemptsCount)
		{
			log.Info($"Ожидаю, если курс {courseId} заблокирован");
			var lockFile = GetCourseLockFile();
			for (var i = 0; i < attemptsCount; i++)
			{
				if (TryCreateLockFile(lockFile))
				{
					log.Info($"Заблокировал курс {courseId}");
					return;
				}

				if (i + 1 == attemptsCount)
				{
					log.Info($"Курс {courseId} заблокирован, не буду ждать");
					return;
				}

				log.Info($"Курс {courseId} заблокирован, жду {waitBetweenLockTries.TotalSeconds} секунд");
				await Task.Delay(waitBetweenLockTries);

				try
				{
					lockFile.Refresh();
					/* If lock-file has been created ago, just delete it and unzip course again */
					if (lockFile.Exists && lockFile.LastWriteTime < DateTime.Now.Subtract(lockLifeTime))
					{
						log.Info($"Курс {courseId} заблокирован слишком давно, снимаю блокировку");

						lockFile.Delete();
					}
				}
				catch (IOException)
				{
				}
			}
		}

		public void ReleaseCourse()
		{
			if (IsLocked)
			{
				GetCourseLockFile().Delete();
				IsLocked = false;
				log.Info($"Разблокировал курс {courseId}");
			}
		}

		private FileInfo GetCourseLockFile()
		{
			return coursesDirectory.GetFile("~" + courseId + ".lock");
		}

		private bool TryCreateLockFile(FileInfo lockFile)
		{
			var tempFileName = Path.GetTempFileName();
			try
			{
				if (!lockFile.Directory.Exists)
					lockFile.Directory.Create();
				new FileInfo(tempFileName).MoveTo(lockFile.FullName);
				IsLocked = true;
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public void Dispose()
		{
			ReleaseCourse();
		}
	}
}