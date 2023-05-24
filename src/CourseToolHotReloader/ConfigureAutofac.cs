using System.Reflection;
using Autofac;
using CourseToolHotReloader.ApiClient;
using CourseToolHotReloader.Application;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Infrastructure;
using CourseToolHotReloader.Menu;
using CourseToolHotReloader.Menu.Commands;
using CourseToolHotReloader.UpdateWorkers;
using CourseToolHotReloader.UpdateWorkers.WatchActionStrategies;

namespace CourseToolHotReloader;

public static class ConfigureAutofac
{
	public static IContainer Build()
	{
		var containerBuilder = new ContainerBuilder();

		containerBuilder.Register(_ => Config.ReadOrCreateConfig().GetValueOrThrow()).As<IConfig>().SingleInstance();

		containerBuilder.RegisterType<ConsoleApplication>().As<IApplication>().SingleInstance();

		containerBuilder.RegisterType<LoginManager>().As<ILoginManager>().SingleInstance();

		containerBuilder.RegisterType<ConsoleLogManager>().As<ILogManager>().SingleInstance();

		containerBuilder.RegisterType<CoursesManager>().As<ICoursesManager>().SingleInstance();
		containerBuilder.RegisterType<CoursesWatcher>().As<ICoursesWatcher>().SingleInstance();
		containerBuilder.RegisterType<CourseUpdateSender>().As<ICourseUpdateSender>().SingleInstance();

		containerBuilder.RegisterType<WatchActionStrategyFactory>().As<IWatchActionStrategyFactory>().SingleInstance();
		containerBuilder.RegisterType<SendFullCourseStrategy>().As<ISendFullCourseStrategy>().InstancePerDependency();
		containerBuilder.RegisterType<SendOnlyChangedStrategy>().As<ISendOnlyChangedStrategy>().InstancePerDependency();
		containerBuilder.RegisterType<CourseUpdateQuery>().As<ICourseUpdateQuery>().InstancePerDependency();
		containerBuilder.RegisterType<ActionDebouncer>().InstancePerDependency();

		containerBuilder.RegisterType<UlearnApiClient>().As<IUlearnApiClient>().SingleInstance();
		containerBuilder.RegisterType<HttpMethods>().As<IHttpMethods>().SingleInstance();

		containerBuilder.RegisterType<ConsoleMenu>().As<IConsoleMenu>().SingleInstance();
		containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
			.AssignableTo<IKeysExtension>()
			.As<IKeysExtension>()
			.SingleInstance();

		containerBuilder.RegisterType<AddCourseCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<RemoveCourseCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<ReloadLastUpdatedCourseCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<ReloadCourseCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<OpenLastUpdatedCourseInBrowserCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<OpenCourseInBrowserCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<LoginCommand>().As<IHotkeyCommand>().SingleInstance();
		containerBuilder.RegisterType<ToggleErrorNotificationsCommand>().As<IHotkeyCommand>().SingleInstance();

		return containerBuilder.Build();
	}
}