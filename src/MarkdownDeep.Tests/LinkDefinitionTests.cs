namespace MarkdownDeep.Tests;

[TestFixture]
internal class LinkDefinitionTests
{
	[SetUp]
	public void Setup()
	{
		r=null;
	}

	[Test]
	public void NoTitle()
	{
		const string str = "[id]: url.com";
		r = LinkDefinition.ParseLinkDefinition(str, false);

		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("url.com"));
			Assert.That(r.title, Is.Null);
		});
	}

	[Test]
	public void DoubleQuoteTitle()
	{
		const string str = "[id]: url.com \"my title\"";
		r = LinkDefinition.ParseLinkDefinition(str, false);

		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("url.com"));
			Assert.That(r.title, Is.EqualTo("my title"));
		});
	}

	[Test]
	public void SingleQuoteTitle()
	{
		const string str = "[id]: url.com \'my title\'";
		r = LinkDefinition.ParseLinkDefinition(str, false);

		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("url.com"));
			Assert.That(r.title, Is.EqualTo("my title"));
		});
	}

	[Test]
	public void ParenthesizedTitle()
	{
		const string str = "[id]: url.com (my title)";
		r = LinkDefinition.ParseLinkDefinition(str, false);

		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("url.com"));
			Assert.That(r.title, Is.EqualTo("my title"));
		});
	}

	[Test]
	public void AngleBracketedUrl()
	{
		const string str = "[id]: <url.com> (my title)";
		r = LinkDefinition.ParseLinkDefinition(str, false);
		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("url.com"));
			Assert.That(r.title, Is.EqualTo("my title"));
		});
	}

	[Test]
	public void MultiLine()
	{
		const string str = "[id]:\n\t     http://www.site.com \n\t      (my title)";
		r = LinkDefinition.ParseLinkDefinition(str, false);

		Assert.Multiple(() =>
		{
			Assert.That(r, Is.Not.Null);
			Assert.That(r.id, Is.EqualTo("id"));
			Assert.That(r.url, Is.EqualTo("http://www.site.com"));
			Assert.That(r.title, Is.EqualTo("my title"));
		});
	}

	[Test]
	public void Invalid()
	{
		Assert.Multiple(() =>
		{
			Assert.That(LinkDefinition.ParseLinkDefinition("[id", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]:", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url> \"title", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url> \'title", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url> (title", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url> \"title\" crap", false), Is.Null);
			Assert.That(LinkDefinition.ParseLinkDefinition("[id]: <url> crap", false), Is.Null);
		});
	}

	private LinkDefinition r;
}