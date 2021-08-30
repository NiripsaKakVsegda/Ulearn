using NUnit.Framework;

namespace Ulearn.Core.Tests
{
	[TestFixture]
	public class TexReplacerTests
	{
		[TestCase(@"Start $\log{n}$ end", @"Start <span class='tex'>\log{n}</span> end")]
		[TestCase("a + 1$ b", @"a + 1$ b")]
		[TestCase("a + 1$ b$", @"a + 1$ b$")]
		[TestCase("a $1 b $2", @"a $1 b $2")]
		[TestCase("a $1 b $ 2", @"a <span class='tex'>1 b </span> 2")]
		[TestCase("a $1 b $2 $", @"a $1 b <span class='tex'>2 </span>")]
		[TestCase(" &#36; ", @" $ ")]
		[TestCase(" &amp;#36; ", @" &#36; ")]
		public void TestParsing(string markdown, string expected)
		{
			var texReplacer = new TexReplacer(markdown);
			var replacedText = texReplacer.ReplacedText;
			var result = texReplacer.PlaceTexInsertsBack(replacedText);
			Assert.AreEqual(expected, result);
		}
	}
}