using NUnit.Framework;
using Ulearn.Core;
using Ulearn.Core.Markdown;
using Markdown = MarkdownDeep.Markdown;

namespace uLearn
{
	[TestFixture]
	public class Md_should
	{
		private static MarkdownRenderContext DefaultMdContext = new MarkdownRenderContext("https://api.ulearn.me", "https://ulearn.me", "BasicProgramming", "Unit1");

		[Test]
		public void qualify_urls()
		{
			Assert.That("[a](a.html)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"https://api.ulearn.me/courses/BasicProgramming/files/Unit1/a.html\""));
			Assert.That("[a](a.html)".RenderMarkdown(DefaultMdContext with { UnitDirectoryRelativeToCourse = "Unit1/" }), Does.Contain("href=\"https://api.ulearn.me/courses/BasicProgramming/files/Unit1/a.html\""));
			Assert.That("[a](/a.html)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"/a.html\""));
			Assert.That("[a](abc)".RenderMarkdown(DefaultMdContext), Does.Contain("href=\"abc\""));
		}

		[Test]
		public void emphasize_underscore()
		{
			Assert.AreEqual(
				"<p><strong>x</strong>,</p>\n",
				new Markdown { ExtraMode = true }.Transform("__x__,"));
		}

		[Test]
		public void dot_emphasize_in_html()
		{
			Assert.AreEqual(
				"<p><span>_x_</span></p>\n",
				new Markdown { ExtraMode = true }.Transform("<span>_x_</span>"));
		}

		[Test]
		public void dot_emphasize_in_html2()
		{
			Assert.AreEqual(
				@"<p><span class=""tex"">noise_V, noise_{\omega}</span></p>",
				@"<span class=""tex"">noise_V, noise_{\omega}</span>".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex()
		{
			Assert.AreEqual(
				@"<p>a <span class='tex'>x</span> b</p>",
				@"a $x$ b".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex1()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>x</span></p>",
				@"$x$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void dont_render_not_separate_dollar()
		{
			Assert.AreEqual(
				@"<p>1$=2$</p>",
				@"1$=2$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_with_spaces_inside()
		{
			Assert.AreEqual(
				@"<p>1 <span class='tex'> = 2 </span></p>",
				@"1 $ = 2 $".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_md_vs_tex()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>a_1=b_2</span></p>",
				@"$a_1=b_2$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void dont_markdown()
		{
			Assert.AreEqual(
				"<div>\n*ha*</div>",
				@"<div markdown='false'>*ha*</div>".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_complex_tex()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>\rho\subset\Sigma^*\times\Sigma^*</span></p>",
				@"$\rho\subset\Sigma^*\times\Sigma^*$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_div()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle x_1=y_1</div>",
				@"$$x_1=y_1$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_div_and_span()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle x_1=y_1</div><p> <span class='tex'>1</span></p>",
				@"$$x_1=y_1$$ $1$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_div_and_div()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div><div class='tex'>\displaystyle 2</div>",
				@"$$1$$
$$2$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_triple_div()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div><div class='tex'>\displaystyle 2</div><div class='tex'>\displaystyle 3</div>",
				@"$$1$$
$$2$$
$$3$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void render_tex_div_surrounded_by_spaces()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div>",
				@" $$1$$ ".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void add_root_url()
		{
			Assert.AreEqual(
				"<p><a href=\"/Link\">Hello world</a></p>\n",
				@"[Hello world](/Link)".RenderMarkdown(DefaultMdContext));
		}
	}
}