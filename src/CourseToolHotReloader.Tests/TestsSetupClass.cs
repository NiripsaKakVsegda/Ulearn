using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.Formatting;

namespace CourseToolHotReloader.Tests
{
	[SetUpFixture]
	public class TestsSetupClass
	{
		[OneTimeSetUp]
		public void GlobalSetup()
		{
			var outputTemplate
				= OutputTemplate.Parse("{Timestamp:HH:mm:ss.fff} {Level:u5} {sourceContext:w}{Message}{NewLine}{Exception}");
			var log = new ConsoleLog(new ConsoleLogSettings { OutputTemplate = outputTemplate }).WithMinimumLevel(LogLevel.Debug);
			LogProvider.Configure(log);
		}
	}
}