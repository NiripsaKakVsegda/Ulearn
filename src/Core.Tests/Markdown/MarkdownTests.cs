using NUnit.Framework;
using Ulearn.Core.Markdown;

namespace Ulearn.Core.Tests.Markdown
{
	[TestFixture]
	public class MarkdownTests
	{
		private static readonly MarkdownRenderContext DefaultMdContext = new("https://api.ulearn.me", "https://ulearn.me", "BasicProgramming", "Unit1");

		[Test]
		public void EmphasizeUnderscore()
		{
			Assert.AreEqual(
				"<p><strong>x</strong>,</p>\n",
				new MarkdownDeep.Markdown { ExtraMode = true }.Transform("__x__,"));
		}

		[Test]
		public void DotEmphasizeInHtml()
		{
			Assert.AreEqual(
				"<p><span>_x_</span></p>\n",
				new MarkdownDeep.Markdown { ExtraMode = true }.Transform("<span>_x_</span>"));
		}

		[Test]
		public void DotEmphasizeInHtml2()
		{
			Assert.AreEqual(
				@"<p><span class=""tex"">noise_V, noise_{\omega}</span></p>",
				@"<span class=""tex"">noise_V, noise_{\omega}</span>".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTex()
		{
			Assert.AreEqual(
				@"<p>a <span class='tex'>x</span> b</p>",
				@"a $x$ b".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTex1()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>x</span></p>",
				@"$x$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void DontRenderNotSeparateDollar()
		{
			Assert.AreEqual(
				@"<p>1$=2$</p>",
				@"1$=2$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexWithSpacesInside()
		{
			Assert.AreEqual(
				@"<p>1 <span class='tex'> = 2 </span></p>",
				@"1 $ = 2 $".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderMdWithTex()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>a_1=b_2</span></p>",
				@"$a_1=b_2$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void DontMarkdown()
		{
			Assert.AreEqual(
				"<div>\n*ha*</div>",
				@"<div markdown='false'>*ha*</div>".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderComplexTex()
		{
			Assert.AreEqual(
				@"<p><span class='tex'>\rho\subset\Sigma^*\times\Sigma^*</span></p>",
				@"$\rho\subset\Sigma^*\times\Sigma^*$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexDiv()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle x_1=y_1</div>",
				@"$$x_1=y_1$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexDivAndSpan()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle x_1=y_1</div><p> <span class='tex'>1</span></p>",
				@"$$x_1=y_1$$ $1$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexDivAndDiv()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div><div class='tex'>\displaystyle 2</div>",
				@"$$1$$
$$2$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexTripleDiv()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div><div class='tex'>\displaystyle 2</div><div class='tex'>\displaystyle 3</div>",
				@"$$1$$
$$2$$
$$3$$".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderTexDivSurroundedBySpaces()
		{
			Assert.AreEqual(
				@"<div class='tex'>\displaystyle 1</div>",
				@" $$1$$ ".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderInlineCode()
		{
			Assert.AreEqual(
				@"<p><code>code</code></p>",
				@"``` code ```".RenderMarkdown(DefaultMdContext).Trim());
		}

		[Test]
		public void RenderCodeBlockWithoutLang()
		{
			Assert.AreEqual(
				"<pre><code>const i = 10;\nConsole.WriteLine(i);</code></pre>",
				(
					"```\n" +
					"const i = 10;\n" +
					"Console.WriteLine(i);\n" +
					"```"
				).RenderMarkdown(DefaultMdContext).Trim());
		}
		
		[Test]
		public void RenderCodeWithLang()
		{
			Assert.AreEqual(
				"<textarea class='code code-sample' data-lang='csharp'>const i = 10;\nConsole.WriteLine(i);</textarea>",
				(
					"```C#\n" +
					"const i = 10;\n" +
					"Console.WriteLine(i);\n" +
					"```"
				).RenderMarkdown(DefaultMdContext).Trim());
		}
		
		[Test]
		public void RenderCodeWithLangOldStyle()
		{
			Assert.AreEqual(
				"<textarea class='code code-sample' data-lang='csharp'>const i = 10;\nConsole.WriteLine(i);</textarea>",
				(
					"```\n" +
					"{{C#}}\n" +
					"const i = 10;\n" +
					"Console.WriteLine(i);\n" +
					"```"
				).RenderMarkdown(DefaultMdContext).Trim());
		}
	}
}