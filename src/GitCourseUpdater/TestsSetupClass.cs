using NUnit.Framework;
using Ulearn.Core.Logging;

namespace GitCourseUpdater
{
	[SetUpFixture]
	public class TestsSetupClass
	{
		[OneTimeSetUp]
		public void GlobalSetup()
		{
			LoggerSetup.SetupForTests();
		}
	}
}