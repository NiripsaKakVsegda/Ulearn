using NUnit.Framework;
using Ulearn.Core.Logging;

namespace Database.Core.Tests
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