using System.IO;
using System.Threading;

namespace CourseToolHotReloader.DirectoryWorkers
{
	public interface ICourseWatcher
	{
		public void StartWatch();
	}

	public class CourseWatcher : ICourseWatcher
	{
		private readonly ISendOnlyChangedStrategy sendOnlyChangedStrategy;
		private readonly ISendFullCourseStrategy sendFullCourseStrategy;
		private readonly IConfig config;

		public CourseWatcher(ISendOnlyChangedStrategy sendOnlyChangedStrategy, ISendFullCourseStrategy sendFullCourseStrategy, IConfig config)
		{
			this.sendOnlyChangedStrategy = sendOnlyChangedStrategy;
			this.sendFullCourseStrategy = sendFullCourseStrategy;
			this.config = config;
		}

		public void StartWatch()
		{
			if (config.SendFullArchive)
			{
				WatchDirectory(config.Path, sendFullCourseStrategy);
			}
			else
			{
				WatchDirectory(config.Path, sendOnlyChangedStrategy);
			}
		}

		private static void WatchDirectory(string directory, IWatchActionStrategy strategy)
		{
			using var watcher = new FileSystemWatcher
			{
				Path = directory,
				NotifyFilter = NotifyFilters.LastWrite
								| NotifyFilters.DirectoryName
								| NotifyFilters.FileName,
				Filter = "*",
				IncludeSubdirectories = true
			};

			watcher.Changed += strategy.Changed;
			watcher.Created += strategy.Created;
			watcher.Deleted += strategy.Deleted;
			watcher.Renamed += strategy.Renamed;

			watcher.EnableRaisingEvents = true;

			Thread.Sleep(Timeout.Infinite);
		}
	}
}