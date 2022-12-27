using NUnit.Framework;

namespace test
{
	[TestFixture]
	[Explicit]
	public class OneFailingTest
	{
		[Test]
		public void I_am_a_failure()
		{
			Assert.Fail();
		}
	}
}
