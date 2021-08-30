using NUnit.Framework;

namespace uLearn.Web.Extensions
{
	[TestFixture]
	public class HtmlExtensions_should
	{
		[Test]
		[TestCase("yandex.ru")]
		[TestCase("google.com")]
		[TestCase("vk.com/hackerdom")]
		[TestCase("http://vk.com")]
		[TestCase("https://t.me/andgein")]
		[TestCase("https://telegram.me/andgein")]
		[TestCase("https://www.google.ru/search?num=30&newwindow=1&q=%D0%BF%D0%BE%D0%B8%D1%81%D0%BA&oq=%D0%BF%D0%BE%D0%B8%D1%81%D0%BA&gs_l=psy-ab.3..0i131k1l3j0l7.1611.1948.0.2070.5.4.0.0.0.0.177.177.0j1.1.0....0...1.1.64.psy-ab..4.1.177....0.H5b9Te8PTRU")]
		[TestCase("http://Math.Min")]
		[TestCase("https://Program.cs")]
		[TestCase("http://filename.txt")]
		[TestCase("https://Result.Add")]
		public void TestUrlRegex(string text)
		{
			Assert.IsTrue(HtmlExtensions.urlRegex.IsMatch(text));
		}

		[Test]
		[TestCase("Math.Round")]
		[TestCase("t.me/andgein")]
		[TestCase("Math.Min")]
		[TestCase("Program.cs")]
		[TestCase("filename.txt")]
		[TestCase("Result.Add")]
		public void TestUrlRegex_shouldNotMatch(string text)
		{
			Assert.IsFalse(HtmlExtensions.urlRegex.IsMatch(text));
		}
	}
}