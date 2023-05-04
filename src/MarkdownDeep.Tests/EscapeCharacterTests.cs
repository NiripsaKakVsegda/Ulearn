namespace MarkdownDeep.Tests;

[TestFixture]
public class EscapeCharacterTests
{
	[SetUp]
	public void SetUp()
	{
		f = new SpanFormatter(new Markdown());
	}

	private SpanFormatter f;

	[Test]
	public void AllEscapeCharacters()
	{
		Assert.That(f.Format(@"pre \\ \` \* \_ \{ \} \[ \] \( \) \# \+ \- \. \! post"), Is.EqualTo(@"pre \ ` * _ { } [ ] ( ) # + - . ! post"));
	}

	[Test]
	public void SomeNonEscapableCharacters()
	{
		Assert.That(f.Format(@"pre \q \% \? post"), Is.EqualTo(@"pre \q \% \? post"));
	}

	[Test]
	public void BackslashWithTwoDashes()
	{
		Assert.That(f.Format(@"backslash with \\-- two dashes"), Is.EqualTo(@"backslash with \-- two dashes"));
	}

	[Test]
	public void BackslashWithGT()
	{
		Assert.That(f.Format(@"backslash with \\> greater"), Is.EqualTo(@"backslash with \&gt; greater"));
	}

	[Test]
	public void EscapeNotALink()
	{
		Assert.That(f.Format(@"\\\[test](not a link)"), Is.EqualTo(@"\[test](not a link)"));
	}

	[Test]
	public void NoEmphasis()
	{
		Assert.That(f.Format(@"\\\*no emphasis*"), Is.EqualTo(@"\*no emphasis*"));
	}
}