using NUnit.Framework;
using Ulearn.Core.Markdown;

namespace Ulearn.Core.Tests.Markdown
{
	[TestFixture]
	public class MarkdownLinksTests
	{
		private static readonly MarkdownRenderContext DefaultMdContext = new MarkdownRenderContext("https://api.ulearn.me", "https://ulearn.me", "BasicProgramming", "Unit1");

		[Test]
		public void QualifyUrls()
		{
			Assert.That("[a](a.html)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"https://api.ulearn.me/courses/BasicProgramming/files/Unit1/a.html\""));
			Assert.That("[a](a.html)".RenderMarkdown(DefaultMdContext with { UnitDirectoryRelativeToCourse = "Unit1/" }), Does.Contain("href=\"https://api.ulearn.me/courses/BasicProgramming/files/Unit1/a.html\""));
			Assert.That("[a](/a.html)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"/a.html\""));
			Assert.That("[a](abc)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"abc\""));
		}

		[Test]
		public void AddRootUrl()
		{
			Assert.AreEqual(
				"<p><a href=\"/Link\">Hello world</a></p>\n",
				@"[Hello world](/Link)".RenderMarkdown(DefaultMdContext));
		}
	}
}