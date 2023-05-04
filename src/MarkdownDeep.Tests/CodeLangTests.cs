namespace MarkdownDeep.Tests;

[TestFixture]
public class CodeLangTests
{
	private Markdown markdown;

	[SetUp]
	public void SetUp()
	{
		markdown = new Markdown
		{
			AutoHeadingIDs = true,
			ExtraMode = true
		};
	}

	[Test]
	public void InlineCode()
	{
		const string source = "```code```";
		var processed = markdown.Transform(source);
		Assert.That(processed, Is.EqualTo("<p><code>code</code></p>\n"));
	}

	[Test]
	public void CodeBlockWithLanguageAndSpaces()
	{
		const string source = "``` csharp \n" +
		                      "code" +
		                      "\n```";
		var processed = markdown.Transform(source);
		Assert.That(processed, Is.EqualTo("<pre><code data-lang=\"csharp\">code\r\n</code></pre>\n\n"));
	}

	[Test]
	public void CodeBlockWithLangAndCustomFormatter()
	{
		const string source = "```csharp\n" +
		                      "code" +
		                      "\n```";
		markdown.FormatCodeBlock += FormatCodePrettyPrint;
		var processed = markdown.Transform(source);
		Assert.That(
			processed
, Is.EqualTo("<textarea class='code code-sample' data-lang='csharp'>code</textarea>\n"));
	}

	private static string FormatCodePrettyPrint(Markdown m, string code, string language)
	{
		code = code.TrimEnd();

		// If not specified, look for a link definition called "default_syntax" and
		// grab the language from its title
		if (language == null)
		{
			var d = m.GetLinkDefinition("default_syntax");
			if (d != null)
				language = d.title;
		}

		// Common replacements
		if (language is "C#" or "cs")
			language = "csharp";
		if (language == "C++")
			language = "cpp";

		// Wrap code in pre/code tags and add PrettyPrint attributes if necessary
		return string.IsNullOrEmpty(language)
			? $"<pre><code>{code}</code></pre>\n"
			: $"<textarea class='code code-sample' data-lang='{language.ToLowerInvariant()}'>{code}</textarea>\n";
	}
}