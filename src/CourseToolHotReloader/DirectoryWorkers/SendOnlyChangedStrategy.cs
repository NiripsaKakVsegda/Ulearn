using System;
using System.IO;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.UpdateQuery;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public interface ISendOnlyChangedStrategy : IWatchActionStrategy
	{
	}

	public class SendOnlyChangedStrategy : ISendOnlyChangedStrategy
	{
		private readonly ICourseUpdateQuery courseUpdateQuery;
		private readonly IConfig config;
		private readonly Action debouncedSendUpdates;

		public SendOnlyChangedStrategy(ICourseUpdateQuery courseUpdateQuery, ICourseUpdateSender courseUpdateSender, IConfig config)
		{
			this.courseUpdateQuery = courseUpdateQuery;
			this.config = config;
			debouncedSendUpdates = ActionHelper.Debounce(() =>
			{
				if (config.PreviousSendHasError)
					courseUpdateSender.SendFullCourse();
				courseUpdateSender.SendCourseUpdates();
			});
		}

		public void Renamed(object sender, RenamedEventArgs e)
		{
			var deletedCourseUpdate = new CourseUpdate(e.OldFullPath);
			courseUpdateQuery.RegisterDelete(deletedCourseUpdate);

			var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
			courseUpdateQuery.RegisterCreate(courseUpdate);

			debouncedSendUpdates();
		}

		public void Deleted(object sender, FileSystemEventArgs e)
		{
			var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
			courseUpdateQuery.RegisterDelete(courseUpdate);
			debouncedSendUpdates();
		}

		public void Created(object sender, FileSystemEventArgs e)
		{
			var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
			courseUpdateQuery.RegisterCreate(courseUpdate);
			debouncedSendUpdates();
		}

		public void Changed(object sender, FileSystemEventArgs e)
		{
			if (Directory.Exists(e.FullPath))
				return;
			var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
			courseUpdateQuery.RegisterUpdate(courseUpdate);
			debouncedSendUpdates();
		}

		private ICourseUpdate BuildCourseUpdateByFileSystemEvent(FileSystemEventArgs fileSystemEventArgs)
		{
			var courseUpdate = new CourseUpdate(fileSystemEventArgs.FullPath);
			return courseUpdate;
		}
	}
}