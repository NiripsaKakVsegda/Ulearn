namespace MarkdownDeep.Tests;

[TestFixture]
internal class TableSpecTests
{
	[SetUp]
	public void SetUp()
	{
	}

	private static TableSpec Parse(string str)
	{
		var s = new StringScanner(str);
		return TableSpec.Parse(s);
	}

	[Test]
	public void Simple()
	{
		var s = Parse("--|--");

		Assert.Multiple(() =>
		{
			Assert.That(s, Is.Not.Null);
			Assert.That(s.LeadingBar, Is.False);
			Assert.That(s.TrailingBar, Is.False);
			Assert.That(s.Columns, Has.Count.EqualTo(2));
			Assert.That(s.Columns[0], Is.EqualTo(ColumnAlignment.Na));
			Assert.That(s.Columns[1], Is.EqualTo(ColumnAlignment.Na));
		});
	}

	[Test]
	public void Alignment()
	{
		var s = Parse("--|:--|--:|:--:");

		Assert.Multiple(() =>
		{
			Assert.That(s, Is.Not.Null);
			Assert.That(s.LeadingBar, Is.False);
			Assert.That(s.TrailingBar, Is.False);
			Assert.That(s.Columns, Has.Count.EqualTo(4));
			Assert.That(s.Columns[0], Is.EqualTo(ColumnAlignment.Na));
			Assert.That(s.Columns[1], Is.EqualTo(ColumnAlignment.Left));
			Assert.That(s.Columns[2], Is.EqualTo(ColumnAlignment.Right));
			Assert.That(s.Columns[3], Is.EqualTo(ColumnAlignment.Center));
		});
	}

	[Test]
	public void LeadingTrailingBars()
	{
		var s = Parse("|--|:--|--:|:--:|");

		Assert.Multiple(() =>
		{
			Assert.That(s, Is.Not.Null);
			Assert.That(s.LeadingBar, Is.True);
			Assert.That(s.TrailingBar, Is.True);
			Assert.That(s.Columns, Has.Count.EqualTo(4));
			Assert.That(s.Columns[0], Is.EqualTo(ColumnAlignment.Na));
			Assert.That(s.Columns[1], Is.EqualTo(ColumnAlignment.Left));
			Assert.That(s.Columns[2], Is.EqualTo(ColumnAlignment.Right));
			Assert.That(s.Columns[3], Is.EqualTo(ColumnAlignment.Center));
		});
	}

	[Test]
	public void Whitespace()
	{
		var s = Parse(" | -- | :-- | --: | :--: |  ");

		Assert.Multiple(() =>
		{
			Assert.That(s, Is.Not.Null);
			Assert.That(s.LeadingBar, Is.True);
			Assert.That(s.TrailingBar, Is.True);
			Assert.That(s.Columns, Has.Count.EqualTo(4));
			Assert.That(s.Columns[0], Is.EqualTo(ColumnAlignment.Na));
			Assert.That(s.Columns[1], Is.EqualTo(ColumnAlignment.Left));
			Assert.That(s.Columns[2], Is.EqualTo(ColumnAlignment.Right));
			Assert.That(s.Columns[3], Is.EqualTo(ColumnAlignment.Center));
		});
	}
}