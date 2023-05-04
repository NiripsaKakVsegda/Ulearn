namespace MarkdownDeep.Tests;

[TestFixture]
internal class HtmlTagTests
{
	[SetUp]
	public void SetUp()
	{
		m_pos = 0;
	}

	[Test]
	public void Unquoted()
	{
		const string str = @"<div x=1 y=2>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(false));
			Assert.That(tag.Attributes, Has.Count.EqualTo(2));
		});
		Assert.Multiple(() =>
		{
			Assert.That(tag.Attributes["x"], Is.EqualTo("1"));
			Assert.That(tag.Attributes["y"], Is.EqualTo("2"));
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void Quoted()
	{
		const string str = @"<div x=""1"" y=""2"">";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(false));
			Assert.That(tag.Attributes, Has.Count.EqualTo(2));
		});
		Assert.Multiple(() =>
		{
			Assert.That(tag.Attributes["x"], Is.EqualTo("1"));
			Assert.That(tag.Attributes["y"], Is.EqualTo("2"));
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void Empty()
	{
		const string str = @"<div>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(false));
			Assert.That(tag.Attributes, Is.Empty);
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void Closed()
	{
		const string str = @"<div/>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(true));
			Assert.That(tag.Attributes, Is.Empty);
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void ClosedWithAttribs()
	{
		const string str = @"<div x=1 y=2/>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(true));
			Assert.That(tag.Attributes, Has.Count.EqualTo(2));
			Assert.That(tag.Attributes["x"], Is.EqualTo("1"));
			Assert.That(tag.Attributes["y"], Is.EqualTo("2"));
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void Closing()
	{
		const string str = @"</div>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("div"));
			Assert.That(tag.Closing, Is.EqualTo(true));
			Assert.That(tag.Closed, Is.EqualTo(false));
			Assert.That(tag.Attributes, Is.Empty);
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void Comment()
	{
		const string str = @"<!-- comment -->";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("!"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(true));
			Assert.That(tag.Attributes, Has.Count.EqualTo(1));
			Assert.That(tag.Attributes["content"], Is.EqualTo(" comment "));
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	[Test]
	public void NonValuedAttribute()
	{
		const string str = @"<iframe y=""2"" allowfullscreen x=""1"" foo>";
		var tag = HtmlTag.Parse(str, ref m_pos);
		Assert.Multiple(() =>
		{
			Assert.That(tag.Name, Is.EqualTo("iframe"));
			Assert.That(tag.Closing, Is.EqualTo(false));
			Assert.That(tag.Closed, Is.EqualTo(false));
			Assert.That(tag.Attributes.Count, Is.EqualTo(4));
			Assert.That(tag.Attributes["allowfullscreen"], Is.EqualTo(""));
			Assert.That(tag.Attributes["foo"], Is.EqualTo(""));
			Assert.That(tag.Attributes["y"], Is.EqualTo("2"));
			Assert.That(tag.Attributes["x"], Is.EqualTo("1"));
			Assert.That(m_pos, Is.EqualTo(str.Length));
		});
	}

	private int m_pos;

}