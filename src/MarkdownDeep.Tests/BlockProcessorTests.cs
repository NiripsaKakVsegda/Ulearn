namespace MarkdownDeep.Tests;

[TestFixture]
internal class BlockProcessorTests
{
	[SetUp]
	public void Setup()
	{
		p = new BlockProcessor(new Markdown(), false);
	}

	[Test]
	public void SingleLineParagraph()
	{
		var b = p.Process("paragraph");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[0].Content, Is.EqualTo("paragraph"));
		});
	}

	[Test]
	public void MultilineParagraph()
	{
		var b = p.Process("l1\nl2\n\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[0].Content, Is.EqualTo("l1\nl2"));
		});
	}

	[Test]
	public void SetExtH1()
	{
		var b = p.Process("heading\n===\n\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.h1));
			Assert.That(b[0].Content, Is.EqualTo("heading"));
		});
	}

	[Test]
	public void SetExtH2()
	{
		var b = p.Process("heading\n---\n\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.h2));
			Assert.That(b[0].Content, Is.EqualTo("heading"));
		});
	}

	[Test]
	public void SetExtHeadingInParagraph()
	{
		var b = p.Process("p1\nheading\n---\np2\n");
		Assert.That(b, Has.Count.EqualTo(3));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[0].Content, Is.EqualTo("p1"));

			Assert.That(b[1].BlockType, Is.EqualTo(BlockType.h2));
			Assert.That(b[1].Content, Is.EqualTo("heading"));

			Assert.That(b[2].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[2].Content, Is.EqualTo("p2"));
		});
	}

	[Test]
	public void AtxHeaders()
	{
		var b = p.Process("#heading#\nparagraph\n");
		Assert.That(b, Has.Count.EqualTo(2));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.h1));
			Assert.That(b[0].Content, Is.EqualTo("heading"));

			Assert.That(b[1].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[1].Content, Is.EqualTo("paragraph"));
		});
	}

	[Test]
	public void AtxHeadingInParagraph()
	{
		var b = p.Process("p1\n## heading ##\np2\n");

		Assert.That(b, Has.Count.EqualTo(3));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[0].Content, Is.EqualTo("p1"));

			Assert.That(b[1].BlockType, Is.EqualTo(BlockType.h2));
			Assert.That(b[1].Content, Is.EqualTo("heading"));

			Assert.That(b[2].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[2].Content, Is.EqualTo("p2"));
		});
	}

	[Test]
	public void CodeBlock()
	{
		var b = p.Process("\tcode1\n\t\tcode2\n\tcode3\nparagraph");
		Assert.That(b, Has.Count.EqualTo(2));

		var cb = b[0];
		Assert.Multiple(() =>
		{
			Assert.That(cb.Content, Is.EqualTo("code1\n\tcode2\ncode3\n"));
			Assert.That(b[1].BlockType, Is.EqualTo(BlockType.p));
			Assert.That(b[1].Content, Is.EqualTo("paragraph"));
		});
	}

	[Test]
	public void HtmlBlock()
	{
		var b = p.Process("<div>\n</div>\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.html));
			Assert.That(b[0].Content, Is.EqualTo("<div>\n</div>\n"));
		});
	}

	[Test]
	public void HtmlCommentBlock()
	{
		var b = p.Process("<!-- this is a\ncomments -->\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.Multiple(() =>
		{
			Assert.That(b[0].BlockType, Is.EqualTo(BlockType.html));
			Assert.That(b[0].Content, Is.EqualTo("<!-- this is a\ncomments -->\n"));
		});
	}

	[Test]
	public void HorizontalRules()
	{
		var b = p.Process("---\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));

		b = p.Process("___\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));

		b = p.Process("***\n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));

		b = p.Process(" - - - \n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));

		b = p.Process("  _ _ _ \n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));

		b = p.Process(" * * * \n");
		Assert.That(b, Has.Count.EqualTo(1));
		Assert.That(b[0].BlockType, Is.EqualTo(BlockType.hr));
	}


	private BlockProcessor p;
}