using NUnit.Framework;

namespace uLearn.NUnitTestRunning.TestsToRun
{
	[TestFixture]
	[Explicit]
	public class TwoTestWithOneTimeSetup
	{
		public static int Counter { get; set; }

		[OneTimeSetUp]
		public void SetUp()
		{
			Counter++;
		}

		[Test]
		public void Test1()
		{
		}

		[Test]
		public void Test2()
		{
		}
	}
}