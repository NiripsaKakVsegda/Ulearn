namespace MarkdownDeep.Tests;

[TestFixture]
public class AutoHeaderIDTests
{
	[SetUp]
	public void SetUp()
	{
		m = new Markdown
		{
			AutoHeadingIDs = true,
			ExtraMode = true
		};
	}

	private Markdown m;

	/* Tests for pandoc style header ID generation */
	/* Tests are based on the examples in the pandoc documentation */

	[Test]
	public void Simple()
	{
		Assert.That(m.MakeUniqueHeaderID(@"Header identifiers in HTML"), Is.EqualTo(@"header-identifiers-in-html"));
	}

	[Test]
	public void WithPunctuation()
	{
		Assert.That(m.MakeUniqueHeaderID(@"Dogs?--in *my* house?"), Is.EqualTo(@"dogs--in-my-house"));
	}

	[Test]
	public void WithLinks()
	{
		Assert.That(m.MakeUniqueHeaderID(@"[HTML](#html), [S5](#S5), [RTF](#rtf)"), Is.EqualTo(@"html-s5-rtf"));
	}

	[Test]
	public void WithLeadingNumbers()
	{
		Assert.That(m.MakeUniqueHeaderID(@"3. Applications"), Is.EqualTo(@"applications"));
	}

	[Test]
	public void RevertToSection()
	{
		Assert.That(m.MakeUniqueHeaderID(@"!!!"), Is.EqualTo(@"section"));
	}

	[Test]
	public void Duplicates()
	{
		Assert.That(m.MakeUniqueHeaderID(@"heading"), Is.EqualTo(@"heading"));
		Assert.That(m.MakeUniqueHeaderID(@"heading"), Is.EqualTo(@"heading-1"));
		Assert.That(m.MakeUniqueHeaderID(@"heading"), Is.EqualTo(@"heading-2"));
	}
}