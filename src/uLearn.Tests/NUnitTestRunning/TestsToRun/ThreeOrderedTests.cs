using NUnit.Framework;

namespace uLearn.NUnitTestRunning.TestsToRun
{
	[TestFixture]
	[Explicit]
	public class ThreeOrderedTests
	{
		public static string Order = "";

		[Test]
		[Order(2)]
		public void Test2()
		{
			Order += "2";
		}

		[Test]
		[Order(1)]
		public void Test1()
		{
			Order += "1";
		}

		[Test]
		[Order(0)]
		public void Test0()
		{
			Order += "0";
		}
	}
}