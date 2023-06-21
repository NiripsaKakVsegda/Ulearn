using System;
using CourseToolHotReloader.Configs;

namespace CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;

public interface IWatchActionStrategyFactory
{
	ISendFullCourseStrategy GetSendFullCourseStrategy(CourseInfo course);
	ISendOnlyChangedStrategy GetSendOnlyChangedStrategy(CourseInfo course);
}

public class WatchActionStrategyFactory : IWatchActionStrategyFactory
{
	private readonly Func<CourseInfo, ISendFullCourseStrategy> sendFullCourseStrategyFactory;
	private readonly Func<CourseInfo, ISendOnlyChangedStrategy> sendOnlyChangedStrategyFactory;

	public WatchActionStrategyFactory(
		Func<CourseInfo, ISendFullCourseStrategy> sendFullCourseStrategyFactory,
		Func<CourseInfo, ISendOnlyChangedStrategy> sendOnlyChangedStrategyFactory
	)
	{
		this.sendFullCourseStrategyFactory = sendFullCourseStrategyFactory;
		this.sendOnlyChangedStrategyFactory = sendOnlyChangedStrategyFactory;
	}

	public ISendFullCourseStrategy GetSendFullCourseStrategy(CourseInfo course) =>
		sendFullCourseStrategyFactory(course);

	public ISendOnlyChangedStrategy GetSendOnlyChangedStrategy(CourseInfo course) =>
		sendOnlyChangedStrategyFactory(course);
}