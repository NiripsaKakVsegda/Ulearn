namespace MarkdownDeep.Tests;

[TestFixture]
public class CodeSpanTests
{
	[SetUp]
	public void SetUp()
	{
		f = new SpanFormatter(new Markdown());
	}

	private SpanFormatter f;

	[Test]
	public void SingleTick()
	{
		Assert.That(f.Format("pre `code span` post"), Is.EqualTo("pre <code>code span</code> post"));
	}

	[Test]
	public void SingleTickWithSpaces()
	{
		Assert.That(f.Format("pre ` code span ` post"), Is.EqualTo("pre <code>code span</code> post"));
	}

	[Test]
	public void MultiTick()
	{
		Assert.That(f.Format("pre ````code span```` post"), Is.EqualTo("pre <code>code span</code> post"));
	}

	[Test]
	public void MultiTickWithEmbeddedTicks()
	{
		Assert.That(f.Format("pre ```` `code span` ```` post"), Is.EqualTo("pre <code>`code span`</code> post"));
	}

	[Test]
	public void ContentEncoded()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("pre ```` <div> ```` post"), Is.EqualTo("pre <code>&lt;div&gt;</code> post"));
			Assert.That(f.Format("pre ```` &amp; ```` post"), Is.EqualTo("pre <code>&amp;amp;</code> post"));
		});
	}
}