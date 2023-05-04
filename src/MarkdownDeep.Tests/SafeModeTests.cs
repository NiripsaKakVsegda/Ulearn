namespace MarkdownDeep.Tests;

[TestFixture]
internal class SafeModeTests
{
	private static IEnumerable<TestCaseData> GetTests()
	{
		return Utils.GetTests("safemode");
	}


	[Test, TestCaseSource(nameof(GetTests))]
	public void Test(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
}