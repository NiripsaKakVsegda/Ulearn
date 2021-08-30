using NUnit.Framework;

namespace uLearn.NUnitTestRunning.TestsToRun
{
	[TestFixture]
	[Explicit]
	public class ThreeTestsWithSetUp
	{
		public static int Counter { get; set; }

		[SetUp]
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

		[Test]
		public void Test3()
		{
		}
	}
}