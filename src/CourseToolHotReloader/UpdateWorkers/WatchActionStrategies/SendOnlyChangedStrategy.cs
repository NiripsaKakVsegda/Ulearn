using System;
using System.IO;
using System.Threading;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;

public interface ISendOnlyChangedStrategy : IWatchActionStrategy
{
}

public class SendOnlyChangedStrategy : ISendOnlyChangedStrategy
{
	private readonly ICourseUpdateQuery courseUpdateQuery;
	private readonly Action debouncedSendUpdates;
	private CancellationTokenSource? cts;

	public SendOnlyChangedStrategy(
		ICourseUpdateQuery courseUpdateQuery,
		ICourseUpdateSender courseUpdateSender,
		CourseInfo course,
		ActionDebouncer actionDebouncer
	)
	{
		this.courseUpdateQuery = courseUpdateQuery;
		debouncedSendUpdates = actionDebouncer.Debounce(() =>
		{
			cts?.Cancel();
			cts = new CancellationTokenSource();
			if (course.CourseState is CourseState.NotSent or CourseState.FatalError)
			{
				this.courseUpdateQuery.Clear();
				courseUpdateSender.SendFullCourseAsync(course);
			}
			else
			{
				courseUpdateSender.SendCourseUpdatesAsync(course, this.courseUpdateQuery, cts.Token);
			}
		});
	}

	public void Changed(object sender, FileSystemEventArgs e)
	{
		if (Directory.Exists(e.FullPath))
			return;
		var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
		courseUpdateQuery.RegisterUpdate(courseUpdate);
		debouncedSendUpdates();
	}

	public void Created(object sender, FileSystemEventArgs e)
	{
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

	public void Renamed(object sender, RenamedEventArgs e)
	{
		var deletedCourseUpdate = new CourseUpdate(e.OldFullPath);
		courseUpdateQuery.RegisterDelete(deletedCourseUpdate);

		var courseUpdate = BuildCourseUpdateByFileSystemEvent(e);
		courseUpdateQuery.RegisterCreate(courseUpdate);

		debouncedSendUpdates();
	}

	private static CourseUpdate BuildCourseUpdateByFileSystemEvent(FileSystemEventArgs fileSystemEventArgs) =>
		new(fileSystemEventArgs.FullPath);
}