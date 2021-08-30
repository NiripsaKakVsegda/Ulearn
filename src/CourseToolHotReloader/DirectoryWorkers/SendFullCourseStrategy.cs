using System;
using System.IO;
using CourseToolHotReloader.UpdateQuery;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public interface ISendFullCourseStrategy : IWatchActionStrategy
	{
	}
	
	public class SendFullCourseStrategy : ISendFullCourseStrategy
	{
		private readonly Action debouncedSendFullCourse;

		public SendFullCourseStrategy(ICourseUpdateSender courseUpdateSender)
		{
			debouncedSendFullCourse = ActionHelper.Debounce(() => courseUpdateSender.SendFullCourse());
		}

		public void Renamed(object sender, RenamedEventArgs e)
		{
			debouncedSendFullCourse();
		}

		public void Deleted(object sender, FileSystemEventArgs e)
		{
			debouncedSendFullCourse();
		}

		public void Created(object sender, FileSystemEventArgs e)
		{
			debouncedSendFullCourse();
		}

		public void Changed(object sender, FileSystemEventArgs e)
		{
			debouncedSendFullCourse();
		}
	}
}