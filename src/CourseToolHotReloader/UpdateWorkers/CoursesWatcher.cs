using System.Collections.Generic;
using System.IO;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;
using Vostok.Logging.Abstractions;

namespace CourseToolHotReloader.UpdateWorkers;

public interface ICoursesWatcher
{
	public void AddWatcher(CourseInfo course);
	public void RemoveWatcher(CourseInfo course);
	public bool HasWatcherFor(CourseInfo course);
}

public class CoursesWatcher : ICoursesWatcher
{
	private readonly IWatchActionStrategyFactory actionStrategyFactory;
	private readonly Dictionary<string, FileSystemWatcher> pathWatchers = new();

	private static ILog Log => LogProvider.Get().ForContext<CoursesWatcher>();

	public CoursesWatcher(IWatchActionStrategyFactory actionStrategyFactory)
	{
		this.actionStrategyFactory = actionStrategyFactory;
	}

	public void AddWatcher(CourseInfo course)
	{
		if (pathWatchers.ContainsKey(course.Path))
			return;

		IWatchActionStrategy strategy = course.SendFullArchive
			? actionStrategyFactory.GetSendFullCourseStrategy(course)
			: actionStrategyFactory.GetSendOnlyChangedStrategy(course);

		pathWatchers.Add(course.Path, InitializeWatcher(course.Path, strategy));
		Log.Info($"Added watcher for course {course.CourseId}, Path: {course.Path}");
	}

	public void RemoveWatcher(CourseInfo course)
	{
		if (!pathWatchers.Remove(course.Path, out var watcher))
			return;

		watcher.EnableRaisingEvents = false;
		watcher.Dispose();
		Log.Info($"Removed watcher for course {course.CourseId}, Path: {course.Path}");
	}

	public bool HasWatcherFor(CourseInfo course) =>
		pathWatchers.ContainsKey(course.Path);

	private static FileSystemWatcher InitializeWatcher(string directory, IWatchActionStrategy strategy)
	{
		var watcher = new FileSystemWatcher
		{
			Path = directory,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName,
			Filter = "*",
			IncludeSubdirectories = true,
		};

		watcher.Changed += strategy.Changed;
		watcher.Created += strategy.Created;
		watcher.Deleted += strategy.Deleted;
		watcher.Renamed += strategy.Renamed;

		watcher.EnableRaisingEvents = true;

		return watcher;
	}
}