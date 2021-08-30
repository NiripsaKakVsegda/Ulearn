using NUnit.Framework;
using Ulearn.Core.Logging;

namespace Ulearn.Core.Tests
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