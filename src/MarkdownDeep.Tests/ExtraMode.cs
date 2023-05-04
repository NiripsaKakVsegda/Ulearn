namespace MarkdownDeep.Tests;

[TestFixture]
internal class ExtraModeTests
{
	private static IEnumerable<TestCaseData> GetTests()
	{
		return Utils.GetTests("extramode");
	}


	[Test, TestCaseSource(nameof(GetTests))]
	public void Test(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
}