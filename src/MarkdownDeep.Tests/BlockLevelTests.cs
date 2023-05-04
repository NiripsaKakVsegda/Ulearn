namespace MarkdownDeep.Tests;

[TestFixture]
internal class BlockLevelTests
{
	private static IEnumerable<TestCaseData> GetTests()
	{
		return Utils.GetTests("blocktests");
	}


	[Test, TestCaseSource(nameof(GetTests))]
	public void Test(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
}