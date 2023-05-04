namespace MarkdownDeep.Tests;

[TestFixture]
public class XssAttackTests
{
	private static bool IsTagReallySafe(HtmlTag tag)
	{
		switch (tag.Name)
		{
			case "B":
			case "UL":
			case "LI":
			case "I":
				return tag.Attributes.Count == 0;

			case "A":
			case "a":
				return tag.Closing && tag.Attributes.Count == 0;
		}

		return false;
	}


	private static IEnumerable<TestCaseData> GetTestsFromFile(string filename)
	{
		var tests = Utils.LoadTextResource("MarkdownDeep.Tests.testfiles.xsstests." + filename);

		// Split into lines
		var lines = tests.Replace("\r\n", "\n").Split('\n');

		// Join bac
		var strings = new List<string>();
		string str = null;
		foreach (var l in lines)
		{
			// Ignore
			if (l.StartsWith("////"))
				continue;

			// Terminator?
			if (l == "====== UNTESTED ======")
			{
				str = null;
				break;
			}

			// Blank line?
			if (string.IsNullOrEmpty(l.Trim()))
			{
				if (str != null)
					strings.Add(str);
				str = null;

				continue;
			}

			if (str == null)
				str = l;
			else
				str = str + "\n" + l;
		}

		if (str != null)
			strings.Add(str);

		return from s in strings select new TestCaseData(s);
	}

	private static IEnumerable<TestCaseData> GetAttacks()
	{
		return GetTestsFromFile("xss_attacks.txt");
	}


	[Test, TestCaseSource(nameof(GetAttacks))]
	public void TestAttacksAreBlocked(string input)
	{
		var p = new StringScanner(input);

		while (!p.Eof)
		{
			var tag = HtmlTag.Parse(p);
			if (tag != null)
			{
				if (tag.IsSafe())
				{
					// There's a few tags that really are safe in the test data
					Assert.That(IsTagReallySafe(tag), Is.True);
				}
			}
			else
			{
				// Next character
				p.SkipForward(1);
			}
		}
	}

	public static IEnumerable<TestCaseData> GetAllowed()
	{
		return GetTestsFromFile("non_attacks.txt");
	}


	[Test, TestCaseSource(nameof(GetAllowed))]
	public void TestNonAttacksAreAllowed(string input)
	{
		var p = new StringScanner(input);

		while (!p.Eof)
		{
			var tag = HtmlTag.Parse(p);
			if (tag != null)
			{
				Assert.That(tag.IsSafe(), Is.True);
			}
			else
			{
				// Next character
				p.SkipForward(1);
			}
		}
	}
}