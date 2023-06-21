using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using CourseToolHotReloader.Application;
using CourseToolHotReloader.Configs;
using CourseToolHotReloader.Infrastructure;
using CourseToolHotReloader.Menu;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
using Vostok.Logging.Formatting;

namespace CourseToolHotReloader;

internal static class Program
{
	private static IContainer container = null!;
	private static IConfig config = null!;
	private static IConsoleMenu consoleMenu = null!;
	private static IApplication application = null!;

	private static void Main()
	{
		try
		{
			Init();
			Startup().Wait();
		}
		catch (Exception e)
		{
			var message = e.GetMessage(config.ApiUrl);
			ConsoleWorker.WriteError(message, e);
		}
		finally
		{
			BeforeProgramEnd();
		}
	}

	private static void Init()
	{
		InitLogger();
		container = ConfigureAutofac.Build();
		config = container.Resolve<IConfig>();
		consoleMenu = container.Resolve<IConsoleMenu>();
		application = container.Resolve<IApplication>();
	}

	private static async Task Startup()
	{
		await application.InitializeAsync();
		application.PrintCourses();
		Console.WriteLine();
		ConsoleWorker.WriteLine("Нажмите любую клавишу чтобы открыть меню.");

		await consoleMenu.StartListeningAsync();
	}

	private static void BeforeProgramEnd()
	{
		FileLog.FlushAll();
	}

	private static void InitLogger()
	{
		var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "{RollingSuffix}.log");
		var fileLogSettings = new FileLogSettings
		{
			FilePath = logPath,
			RollingStrategy = new RollingStrategyOptions
			{
				MaxFiles = 0,
				Type = RollingStrategyType.Hybrid,
				Period = RollingPeriod.Day,
				MaxSize = 4 * 1073741824L,
			},
			OutputTemplate = OutputTemplate.Parse("{Timestamp:HH:mm:ss.fff} {Level:u5} {sourceContext:w}{Message}{NewLine}{Exception}")
		};
		var fileLog = new FileLog(fileLogSettings).WithMinimumLevel(LogLevel.Info);
		LogProvider.Configure(fileLog);
	}
}