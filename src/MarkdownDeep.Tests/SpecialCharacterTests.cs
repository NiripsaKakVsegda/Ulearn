namespace MarkdownDeep.Tests;

[TestFixture]
public class SpecialCharacterTests
{
	[SetUp]
	public void SetUp()
	{
		f = new SpanFormatter(new Markdown());
	}

	private SpanFormatter f;

	[Test]
	public void SimpleTag()
	{
		Assert.That(f.Format("pre <a> post"), Is.EqualTo("pre <a> post"));
	}

	[Test]
	public void TagWithAttributes()
	{
		Assert.That(f.Format("pre <a href=\"somewhere.html\" target=\"_blank\">link</a> post"), Is.EqualTo("pre <a href=\"somewhere.html\" target=\"_blank\">link</a> post"));
	}

	[Test]
	public void NotATag()
	{
		Assert.That(f.Format("pre a < b post"), Is.EqualTo("pre a &lt; b post"));
	}

	[Test]
	public void NotATag2()
	{
		Assert.That(f.Format("pre a<b post"), Is.EqualTo("pre a&lt;b post"));
	}

	[Test]
	public void AmpersandsInUrls()
	{
		Assert.That(f.Format("pre <a href=\"somewhere.html?arg1=a&arg2=b\" target=\"_blank\">link</a> post"), Is.EqualTo("pre <a href=\"somewhere.html?arg1=a&amp;arg2=b\" target=\"_blank\">link</a> post"));
	}

	[Test]
	public void AmpersandsInParagraphs()
	{
		Assert.That(f.Format("pre this & that post"), Is.EqualTo("pre this &amp; that post"));
	}

	[Test]
	public void HtmlEntities()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("pre &amp; post"), Is.EqualTo("pre &amp; post"));
			Assert.That(f.Format("pre &#123; post"), Is.EqualTo("pre &#123; post"));
			Assert.That(f.Format("pre &#x1aF; post"), Is.EqualTo("pre &#x1aF; post"));
		});
	}

	[Test]
	public void EscapeChars()
	{
		Assert.That(f.Format(@"\\ \` \* \_ \{ \} \[ \] \( \) \# \+ \- \. \! \>"), Is.EqualTo(@"\ ` * _ { } [ ] ( ) # + - . ! &gt;"));
	}


}