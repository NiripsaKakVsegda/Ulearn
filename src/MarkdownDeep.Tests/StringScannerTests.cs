namespace MarkdownDeep.Tests;

[TestFixture]
internal class StringScannerTests
{
	[Test]
	public void Tests()
	{
		var p = new StringScanner();

		p.Reset("This is a string with something [bracketed]");
		Assert.Multiple(() =>
		{
			Assert.That(p.Bof, Is.True);
			Assert.That(p.Eof, Is.False);
			Assert.That(p.SkipString("This"), Is.True);
			Assert.That(p.Bof, Is.False);
			Assert.That(p.Eof, Is.False);
			Assert.That(p.SkipString("huh?"), Is.False);
			Assert.That(p.SkipLineSpace(), Is.True);
			Assert.That(p.SkipChar('i'), Is.True);
			Assert.That(p.SkipChar('s'), Is.True);
			Assert.That(p.SkipWhitespace(), Is.True);
			Assert.That(p.DoesMatchAny(new[] { 'r', 'a', 't'} ), Is.True);
			Assert.That(p.Find("Not here"), Is.False);
			Assert.That(p.Find("WITH"), Is.False);
			Assert.That(p.FindI("Not here"), Is.False);
			Assert.That(p.FindI("WITH"), Is.True);
			Assert.That(p.Find('['), Is.True);
		});
		p.SkipForward(1);
		p.Mark();
		Assert.Multiple(() =>
		{
			Assert.That(p.Find(']'), Is.True);
			Assert.That(p.Extract(), Is.EqualTo("bracketed"));
			Assert.That(p.SkipChar(']'), Is.True);
			Assert.That(p.Eof, Is.True);
		});
	}
}