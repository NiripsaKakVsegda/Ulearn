using System;
using NUnit.Framework;
using Ulearn.Common;

namespace Ulearn.Core.Tests
{
	[TestFixture]
	public class LanguageTests
	{
		[Test]
		public void TestParsing()
		{
			Assert.AreEqual(Language.CSharp, Enum.Parse<Language>("csharp", true));
		}
	}
}