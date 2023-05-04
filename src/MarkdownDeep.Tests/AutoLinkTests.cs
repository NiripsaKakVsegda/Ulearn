namespace MarkdownDeep.Tests;

[TestFixture]
internal class AutoLinkTests
{
	[SetUp]
	public void SetUp()
	{
		m = new Markdown();
		s = new SpanFormatter(m);
	}

	[Test]
	public void http()
	{
		Assert.That(s.Format("pre <http://url.com> post"), Is.EqualTo("pre <a href=\"http://url.com\">http://url.com</a> post"));
	}

	[Test]
	public void https()
	{
		Assert.That(s.Format("pre <https://url.com> post"), Is.EqualTo("pre <a href=\"https://url.com\">https://url.com</a> post"));
	}

	[Test]
	public void ftp()
	{
		Assert.That(s.Format("pre <ftp://url.com> post"), Is.EqualTo("pre <a href=\"ftp://url.com\">ftp://url.com</a> post"));
	}

	private Markdown m;
	private SpanFormatter s;
}