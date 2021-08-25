using NUnit.Framework;
using Ulearn.Core.Markdown;

namespace Ulearn.Core.Tests.Markdown
{
	[TestFixture]
	public class MarkdownLinksTests
	{
		private static readonly MarkdownRenderContext defaultMdContext = new MarkdownRenderContext("https://api.ulearn.me", "https://ulearn.me", "BasicProgramming", "Unit1");
		private const string apiFilePrefix = "https://api.ulearn.me/courses/BasicProgramming/files";

		[Test]
		public void SimpleUrls()
		{
			Assert.That("[a](a.pdf)".RenderMarkdown(defaultMdContext), Does.Contain($"href=\"{apiFilePrefix}/Unit1/a.pdf\""));
			Assert.That("[a](a.pdf)".RenderMarkdown(defaultMdContext with { UnitDirectoryRelativeToCourse = "" }), Does.Contain($"href=\"{apiFilePrefix}/a.pdf\""));
			Assert.That("[a](abc)".RenderMarkdown(defaultMdContext), Does.Contain("href=\"abc\""));
		}

		[Test]
		public void RootUrls()
		{
			Assert.That("[a](/link)".RenderMarkdown(defaultMdContext), Does.Contain("href=\"/link\""));
			Assert.That("[a](/a.pdf)".RenderMarkdown(defaultMdContext), Does.Contain("href=\"/a.pdf\""));
		}

		[Test]
		public void UrlsWithWithLinkToDirectoryAboveUnit()
		{
			Assert.That("[a](../a.pdf)".RenderMarkdown(defaultMdContext), Does.Contain($"href=\"{apiFilePrefix}/a.pdf\""));
		}

		[Test]
		public void UrlWithLinkToAnotherUnit()
		{
			Assert.That("[a](../Unit2/a.pdf)".RenderMarkdown(defaultMdContext), Does.Contain($"href=\"{apiFilePrefix}/Unit2/a.pdf\""));
		}

		[Test]
		public void UrlWithDot()
		{
			Assert.That("[a](./link)".RenderMarkdown(defaultMdContext), Does.Contain("href=\"link\""));
			Assert.That("[a](./a.pdf)".RenderMarkdown(defaultMdContext), Does.Contain($"href=\"{apiFilePrefix}/Unit1/a.pdf\""));
		}
	}
}