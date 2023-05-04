namespace MarkdownDeep.Tests;

[TestFixture]
internal class LinkAndImgTests
{
	[SetUp]
	public void SetUp()
	{
		m = new Markdown();
		m.AddLinkDefinition(new LinkDefinition("link1", "url.com", "title"));
		m.AddLinkDefinition(new LinkDefinition("link2", "url.com"));
		m.AddLinkDefinition(new LinkDefinition("img1", "url.com/image.png", "title"));
		m.AddLinkDefinition(new LinkDefinition("img2", "url.com/image.png"));

		s = new SpanFormatter(m);
	}

	[Test]
	public void ReferenceLinkWithTitle()
	{
		Assert.That(s.Format("pre [link text][link1] post"), Is.EqualTo("pre <a href=\"url.com\" title=\"title\">link text</a> post"));
	}

	[Test]
	public void ReferenceLinkIdsAreCaseInsensitive()
	{
		Assert.That(s.Format("pre [link text][LINK1] post"), Is.EqualTo("pre <a href=\"url.com\" title=\"title\">link text</a> post"));
	}

	[Test]
	public void ImplicitReferenceLinkWithoutTitle()
	{
		Assert.Multiple(() =>
		{
			Assert.That(s.Format("pre [link2] post"), Is.EqualTo("pre <a href=\"url.com\">link2</a> post"));
			Assert.That(s.Format("pre [link2][] post"), Is.EqualTo("pre <a href=\"url.com\">link2</a> post"));
		});
	}

	[Test]
	public void ImplicitReferenceLinkWithTitle()
	{
		Assert.Multiple(() =>
		{
			Assert.That(s.Format("pre [link1] post"), Is.EqualTo("pre <a href=\"url.com\" title=\"title\">link1</a> post"));
			Assert.That(s.Format("pre [link1][] post"), Is.EqualTo("pre <a href=\"url.com\" title=\"title\">link1</a> post"));
		});
	}

	[Test]
	public void ReferenceLinkWithoutTitle()
	{
		Assert.That(s.Format("pre [link text][link2] post"), Is.EqualTo("pre <a href=\"url.com\">link text</a> post"));
	}

	[Test]
	public void MissingReferenceLink()
	{
		Assert.That(s.Format("pre [link text][missing] post"), Is.EqualTo("pre [link text][missing] post"));
	}

	[Test]
	public void InlineLinkWithTitle()
	{
		Assert.That(s.Format("pre [link text](url.com \"title\") post"), Is.EqualTo("pre <a href=\"url.com\" title=\"title\">link text</a> post"));
	}

	[Test]
	public void InlineLinkWithoutTitle()
	{
		Assert.That(s.Format("pre [link text](url.com) post"), Is.EqualTo("pre <a href=\"url.com\">link text</a> post"));
	}

	[Test]
	public void Boundaries()
	{
		Assert.Multiple(() =>
		{
			Assert.That(s.Format("[link text](url.com)"), Is.EqualTo("<a href=\"url.com\">link text</a>"));
			Assert.That(s.Format("[link text][link1]"), Is.EqualTo("<a href=\"url.com\" title=\"title\">link text</a>"));
		});
	}

	[Test]
	public void ReferenceImgWithTitle()
	{
		Assert.That(s.Format("pre ![alt text][img1] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"alt text\" title=\"title\" /> post"));
	}

	[Test]
	public void ImplicitReferenceImgWithoutTitle()
	{
		Assert.Multiple(() =>
		{
			Assert.That(s.Format("pre ![img2] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"img2\" /> post"));
			Assert.That(s.Format("pre ![img2][] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"img2\" /> post"));
		});
	}

	[Test]
	public void ImplicitReferenceImgWithTitle()
	{
		Assert.Multiple(() =>
		{
			Assert.That(s.Format("pre ![img1] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"img1\" title=\"title\" /> post"));
			Assert.That(s.Format("pre ![img1][] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"img1\" title=\"title\" /> post"));
		});
	}

	[Test]
	public void ReferenceImgWithoutTitle()
	{
		Assert.That(s.Format("pre ![alt text][img2] post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"alt text\" /> post"));
	}

	[Test]
	public void MissingReferenceImg()
	{
		Assert.That(s.Format("pre ![alt text][missing] post"), Is.EqualTo("pre ![alt text][missing] post"));
	}

	[Test]
	public void InlineImgWithTitle()
	{
		Assert.That(s.Format("pre ![alt text](url.com/image.png \"title\") post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"alt text\" title=\"title\" /> post"));
	}

	[Test]
	public void InlineImgWithoutTitle()
	{
		Assert.That(s.Format("pre ![alt text](url.com/image.png) post"), Is.EqualTo("pre <img src=\"url.com/image.png\" alt=\"alt text\" /> post"));
	}


	[Test]
	public void ImageLink() 
	{
		Assert.That(s.Format("pre [![alt text](url.com/image.png)](url.com) post"), Is.EqualTo("pre <a href=\"url.com\"><img src=\"url.com/image.png\" alt=\"alt text\" /></a> post"));
	}

	private Markdown m;
	private SpanFormatter s;
}