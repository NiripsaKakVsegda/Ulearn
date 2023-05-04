using System.Text;
using System.Reflection;

namespace MarkdownDeep.Tests;

public static class Utils
{
	public static IEnumerable<TestCaseData> GetTests(string foldername)
	{
		return from name in Assembly.GetExecutingAssembly().GetManifestResourceNames()
			where name.StartsWith("MarkdownDeep.Tests.testfiles." + foldername + ".") && (name.EndsWith(".txt") || name.EndsWith(".text"))
			select new TestCaseData(name);
	}

	public static string LoadTextResource(string name)
	{
		// get a reference to the current assembly
		var a = Assembly.GetExecutingAssembly();
		var r = new StreamReader(a.GetManifestResourceStream(name)!);
		var str = r.ReadToEnd();
		r.Close();

		return str;
	}


	public static string strip_redundant_whitespace(string str)
	{
		var sb = new StringBuilder();

		str = str.Replace("\r\n", "\n");

		var i = 0;
		while (i < str.Length)
		{
			var ch = str[i];
			switch (ch)
			{
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					// Store start of white space
					i++;

					// Find end of whitespace
					while (i < str.Length)
					{
						ch = str[i];
						if (ch != ' ' && ch != '\t' && ch != '\r' && ch != '\n')
							break;

						i++;
					}

					// Replace with a single space
					if (i < str.Length && str[i] != '<')
						sb.Append(' ');

					break;

				case '>':
					sb.Append("> ");
					i++;
					while (i < str.Length)
					{
						ch = str[i];
						if (ch != ' ' && ch != '\t' && ch != '\r' && ch != '\n')
							break;

						i++;
					}

					break;

				case '<':
					if (i + 5 < str.Length && str.Substring(i, 5) == "<pre>")
					{
						sb.Append(' ');

						// Special handling for pre blocks

						// Find end
						var end = str.IndexOf("</pre>", i, StringComparison.Ordinal);
						if (end < 0)
							end = str.Length;

						// Append the pre block
						sb.Append(str, i, end - i);
						sb.Append(' ');

						// Jump to end
						i = end;
					}
					else
					{
						sb.Append(" <");
						i++;
					}

					break;

				default:
					sb.Append(ch);
					i++;
					break;
			}
		}

		return sb.ToString().Trim();
	}

	public static void RunResourceTest(string resourceName)
	{
		var input = LoadTextResource(resourceName);
		var expected = LoadTextResource(Path.ChangeExtension(resourceName, "html"));

		var md = new Markdown
		{
			SafeMode = resourceName.Contains("(SafeMode)"),
			ExtraMode = resourceName.Contains("(ExtraMode)"),
			MarkdownInHtml = resourceName.Contains("(MarkdownInHtml)"),
			AutoHeadingIDs = resourceName.Contains("(AutoHeadingIDs)")
		};

		var actual = md.Transform(input);
		var actual_clean = strip_redundant_whitespace(actual);
		var expected_clean = strip_redundant_whitespace(expected);

		var sep = new string('-', 30) + "\n";

		Console.WriteLine("Input:\n" + sep + input);
		Console.WriteLine("Actual:\n" + sep + actual);
		Console.WriteLine("Expected:\n" + sep + expected);

		Assert.That(actual_clean, Is.EqualTo(expected_clean));
	}
}