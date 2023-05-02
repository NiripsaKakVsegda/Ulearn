namespace MarkdownDeep.Tests;

[TestFixture]
internal class MoreTestFiles
{
	private static IEnumerable<TestCaseData> GetTests_mdtest11()
	{
		return Utils.GetTests("mdtest11");
	}


	[Test, TestCaseSource(nameof(GetTests_mdtest11))]
	public void Test_mdtest11(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}

	private static IEnumerable<TestCaseData> GetTests_mdtest01()
	{
		return Utils.GetTests("mdtest01");
	}


	[Test, TestCaseSource(nameof(GetTests_mdtest01))]
	public void Test_mdtest01(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}

	/*
	* Don't run the pandoc test's as they're basically a demonstration of things
	* that are broken in markdown.
	* 
	public static IEnumerable<TestCaseData> GetTests_pandoc()
	{
		return Utils.GetTests("pandoc");
	}


	[Test, TestCaseSource("GetTests_pandoc")]
	public void Test_pandoc(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
	*/

	private static IEnumerable<TestCaseData> GetTests_phpmarkdown()
	{
		return Utils.GetTests("phpmarkdown");
	}


	[Test, TestCaseSource(nameof(GetTests_phpmarkdown))]
	public void Test_phpmarkdown(string resourceName)
	{
		Utils.RunResourceTest(resourceName);
	}
}