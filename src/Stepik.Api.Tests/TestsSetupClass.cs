using NUnit.Framework;
using Ulearn.Core.Logging;

namespace Stepik.Api.Tests
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