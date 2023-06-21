using System;
using System.IO;
using System.Threading;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Infrastructure;

namespace CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;

public interface ISendFullCourseStrategy : IWatchActionStrategy
{
}

public class SendFullCourseStrategy : ISendFullCourseStrategy
{
	private readonly Action debouncedSendFullCourse;
	private CancellationTokenSource? cts;

	public SendFullCourseStrategy(ICourseUpdateSender courseUpdateSender, CourseInfo course, ActionDebouncer actionDebouncer)
	{
		debouncedSendFullCourse = actionDebouncer.Debounce(() =>
		{
			cts?.Cancel();
			cts = new CancellationTokenSource();
			courseUpdateSender.SendFullCourseAsync(course, cts.Token);
		});
	}

	public void Changed(object sender, FileSystemEventArgs e)
	{
		debouncedSendFullCourse();
	}

	public void Created(object sender, FileSystemEventArgs e)
	{
		debouncedSendFullCourse();
	}

	public void Deleted(object sender, FileSystemEventArgs e)
	{
		debouncedSendFullCourse();
	}

	public void Renamed(object sender, RenamedEventArgs e)
	{
		debouncedSendFullCourse();
	}
}