namespace MarkdownDeep.Tests;

[TestFixture]
public class EmphasisTests
{
	[SetUp]
	public void SetUp()
	{
		f = new SpanFormatter(new Markdown());
	}

	private SpanFormatter f;

	[Test]
	public void PlainText()
	{
		Assert.That(f.Format("This is plain text"), Is.EqualTo("This is plain text"));
	}


	[Test]
	public void em_simple()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("This is *em* text"), Is.EqualTo("This is <em>em</em> text"));
			Assert.That(f.Format("This is _em_ text"), Is.EqualTo("This is <em>em</em> text"));
		});
	}

	[Test]
	public void strong_simple()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("This is **strong** text"), Is.EqualTo("This is <strong>strong</strong> text"));
			Assert.That(f.Format("This is __strong__ text"), Is.EqualTo("This is <strong>strong</strong> text"));
		});
	}

	[Test]
	public void em_strong_lead_tail()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("__strong__"), Is.EqualTo("<strong>strong</strong>"));
			Assert.That(f.Format("**strong**"), Is.EqualTo("<strong>strong</strong>"));
			Assert.That(f.Format("_em_"), Is.EqualTo("<em>em</em>"));
			Assert.That(f.Format("*em*"), Is.EqualTo("<em>em</em>"));
		});
	}

	[Test]
	public void strongem()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("***strongem***"), Is.EqualTo("<strong><em>strongem</em></strong>"));
			Assert.That(f.Format("___strongem___"), Is.EqualTo("<strong><em>strongem</em></strong>"));
		});
	}

	[Test]
	public void no_strongem_if_spaces()
	{
		Assert.Multiple(() =>
		{
			Assert.That(f.Format("pre * notem *"), Is.EqualTo("pre * notem *"));
			Assert.That(f.Format("pre ** notstrong **"), Is.EqualTo("pre ** notstrong **"));
			Assert.That(f.Format("pre *Apples *Bananas *Oranges"), Is.EqualTo("pre *Apples *Bananas *Oranges"));
		});
	}

	[Test]
	public void em_in_word()
	{
		Assert.That(f.Format("un*frigging*believable"), Is.EqualTo("un<em>frigging</em>believable"));
	}

	[Test]
	public void strong_in_word()
	{
		Assert.That(f.Format("un**frigging**believable"), Is.EqualTo("un<strong>frigging</strong>believable"));
	}

	[Test]
	public void combined_1()
	{
		Assert.That(f.Format("***test test***"), Is.EqualTo("<strong><em>test test</em></strong>"));
	}

	[Test]
	public void combined_2()
	{
		Assert.That(f.Format("___test test___"), Is.EqualTo("<strong><em>test test</em></strong>"));
	}


	[Test]
	public void combined_3()
	{
		Assert.That(f.Format("*test **test***"), Is.EqualTo("<em>test <strong>test</strong></em>"));
	}


	[Test]
	public void combined_4()
	{
		Assert.That(f.Format("**test *test***"), Is.EqualTo("<strong>test <em>test</em></strong>"));
	}


	[Test]
	public void combined_5()
	{
		Assert.That(f.Format("***test* test**"), Is.EqualTo("<strong><em>test</em> test</strong>"));
	}


	[Test]
	public void combined_6()
	{
		Assert.That(f.Format("***test** test*"), Is.EqualTo("<em><strong>test</strong> test</em>"));
	}


	[Test]
	public void combined_7()
	{
		Assert.That(f.Format("***test* test**"), Is.EqualTo("<strong><em>test</em> test</strong>"));
	}


	[Test]
	public void combined_8()
	{
		Assert.That(f.Format("**test *test***"), Is.EqualTo("<strong>test <em>test</em></strong>"));
	}


	[Test]
	public void combined_9()
	{
		Assert.That(f.Format("*test **test***"), Is.EqualTo("<em>test <strong>test</strong></em>"));
	}


	[Test]
	public void combined_10()
	{
		Assert.That(f.Format("_test __test___"), Is.EqualTo("<em>test <strong>test</strong></em>"));
	}


	[Test]
	public void combined_11()
	{
		Assert.That(f.Format("__test _test___"), Is.EqualTo("<strong>test <em>test</em></strong>"));
	}


	[Test]
	public void combined_12()
	{
		Assert.That(f.Format("___test_ test__"), Is.EqualTo("<strong><em>test</em> test</strong>"));
	}


	[Test]
	public void combined_13()
	{
		Assert.That(f.Format("___test__ test_"), Is.EqualTo("<em><strong>test</strong> test</em>"));
	}


	[Test]
	public void combined_14()
	{
		Assert.That(f.Format("___test_ test__"), Is.EqualTo("<strong><em>test</em> test</strong>"));
	}


	[Test]
	public void combined_15()
	{
		Assert.That(f.Format("__test _test___"), Is.EqualTo("<strong>test <em>test</em></strong>"));
	}


	[Test]
	public void combined_16()
	{
		Assert.That(f.Format("_test __test___"), Is.EqualTo("<em>test <strong>test</strong></em>"));
	}

	[Test]
	public void combined_17()
	{
		var fExtra = new SpanFormatter(new Markdown() { ExtraMode = true });
		Assert.That(fExtra.Format("__Bold__ _Italic_"), Is.EqualTo("<strong>Bold</strong> <em>Italic</em>"));
	}

	[Test]
	public void combined_18()
	{
		var fExtra = new SpanFormatter(new Markdown() { ExtraMode = true });
		Assert.That(fExtra.Format("_Emphasis_, trailing"), Is.EqualTo("<em>Emphasis</em>, trailing"));
	}



}