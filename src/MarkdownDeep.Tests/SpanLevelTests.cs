namespace MarkdownDeep.Tests;

[TestFixture]
internal class SpanLevelTests
{
	private static IEnumerable<TestCaseData> GetTests()
	{
		return Utils.GetTests("spantests");
	}


	[Test, TestCaseSource(nameof(GetTests))]
	public void Test(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
}