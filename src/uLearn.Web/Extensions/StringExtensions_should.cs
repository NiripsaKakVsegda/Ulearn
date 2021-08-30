using NUnit.Framework;

namespace uLearn.Web.Extensions
{
	[TestFixture]
	public class StringExtensions_should
	{
		[Test]
		public void TruncateHtmlWithEllipsis_shouldAutoCloseTags()
		{
			var html = "Text before tag <b>text in tag</b> text after tag";
			var truncated = html.TruncateHtmlWithEllipsis(25);
			Assert.True(truncated.EndsWith("...</b>"));
		}

		[Test]
		public void TruncateHtmlWithEllipsis_shouldIgnoreHtmlEntities()
		{
			var html = "Text before entity &amp; text after entity";
			var truncated = html.TruncateHtmlWithEllipsis(20);
			Assert.True(truncated.Contains("&amp;"));
		}
	}
}