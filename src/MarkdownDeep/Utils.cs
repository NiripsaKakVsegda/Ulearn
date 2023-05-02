//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

using System.Text;

namespace MarkdownDeep;

/*
 * Various utility and extension methods
 */
internal static class Utils
{
	// Extension method.  Remove last item from a list
	public static T Pop<T>(this List<T> list) where T : class
	{
		if (list.Count == 0)
			return null;

		var val = list[^1];
		list.RemoveAt(list.Count - 1);
		return val;
	}


	// Scan a string for a valid identifier.  Identifier must start with alpha or underscore
	// and can be followed by alpha, digit or underscore
	// Updates `pos` to character after the identifier if matched
	public static bool ParseIdentifier(string str, ref int pos, ref string identifier)
	{
		if (pos >= str.Length)
			return false;

		// Must start with a letter or underscore
		if (!char.IsLetter(str[pos]) && str[pos] != '_')
		{
			return false;
		}

		// Find the end
		var startPos = pos;
		pos++;
		while (pos < str.Length && (char.IsDigit(str[pos]) || char.IsLetter(str[pos]) || str[pos] == '_'))
			pos++;

		// Return it
		identifier = str.Substring(startPos, pos - startPos);
		return true;
	}

	// Skip over anything that looks like a valid html entity (eg: &amp, &#123, &#nnn) etc...
	// Updates `pos` to character after the entity if matched
	public static bool SkipHtmlEntity(string str, ref int pos, ref string entity)
	{
		if (str[pos] != '&')
			return false;

		var savePos = pos;
		var len = str.Length;
		var i = pos + 1;

		// Number entity?
		var bNumber = false;
		var bHex = false;
		if (i < len && str[i] == '#')
		{
			bNumber = true;
			i++;

			// Hex identity?
			if (i < len && (str[i] == 'x' || str[i] == 'X'))
			{
				bHex = true;
				i++;
			}
		}

		// Parse the content
		var contentPos = i;
		while (i < len)
		{
			var ch = str[i];

			if (bHex)
			{
				if (!(char.IsDigit(ch) || ch is >= 'a' and <= 'f' || ch is >= 'A' and <= 'F'))
					break;
			}

			else if (bNumber)
			{
				if (!char.IsDigit(ch))
					break;
			}
			else if (!char.IsLetterOrDigit(ch))
				break;

			i++;
		}

		// Quit if ran out of string
		if (i == len)
			return false;

		// Quit if nothing in the content
		if (i == contentPos)
			return false;

		// Quit if didn't find a semicolon
		if (str[i] != ';')
			return false;

		// Looks good...
		pos = i + 1;

		entity = str.Substring(savePos, pos - savePos);
		return true;
	}

	// Randomize a string using html entities;
	public static void HtmlRandomize(StringBuilder dest, string str)
	{
		// Deterministic random seed
		var seed = 0;
		foreach (var ch in str)
		{
			seed = unchecked(seed + ch);
		}

		var r = new Random(seed);

		// Randomize
		foreach (var ch in str)
		{
			var x = r.Next() % 100;
			switch (x)
			{
				case > 90 when ch != '@':
					dest.Append(ch);
					break;
				case > 45:
					dest.Append("&#");
					dest.Append((int)ch);
					dest.Append(';');
					break;
				default:
					dest.Append("&#x");
					dest.Append(((int)ch).ToString("x"));
					dest.Append(';');
					break;
			}
		}
	}

	// Like HtmlEncode, but don't escape &'s that look like html entities
	public static void SmartHtmlEncodeAmpsAndAngles(StringBuilder dest, string str)
	{
		if (str == null)
			return;

		for (var i = 0; i < str.Length; i++)
		{
			switch (str[i])
			{
				case '&':
					var start = i;
					string unused = null;
					if (SkipHtmlEntity(str, ref i, ref unused))
					{
						dest.Append(str, start, i - start);
						i--;
					}
					else
					{
						dest.Append("&amp;");
					}

					break;

				case '<':
					dest.Append("&lt;");
					break;

				case '>':
					dest.Append("&gt;");
					break;

				case '\"':
					dest.Append("&quot;");
					break;

				default:
					dest.Append(str[i]);
					break;
			}
		}
	}


	// Like HtmlEncode, but only escape &'s that don't look like html entities
	public static void SmartHtmlEncodeAmps(StringBuilder dest, string str, int startOffset, int len)
	{
		var end = startOffset + len;
		for (var i = startOffset; i < end; i++)
		{
			switch (str[i])
			{
				case '&':
					var start = i;
					string unused = null;
					if (SkipHtmlEntity(str, ref i, ref unused))
					{
						dest.Append(str, start, i - start);
						i--;
					}
					else
					{
						dest.Append("&amp;");
					}

					break;

				default:
					dest.Append(str[i]);
					break;
			}
		}
	}

	// Check if a string is in an array of strings
	public static bool IsInList(string str, IEnumerable<string> list)
	{
		return list.Any(t => string.CompareOrdinal(t, str) == 0);
	}

	// Check if a url is "safe" (we require urls start with valid protocol)
	// Definitely don't allow "javascript:" or any of it's encodings.
	public static bool IsSafeUrl(string url)
	{
		return url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://");
	}

	// Check if a character is escapable in markdown
	public static bool IsEscapableChar(char ch, bool extraMode)
	{
		switch (ch)
		{
			case '\\':
			case '`':
			case '*':
			case '_':
			case '{':
			case '}':
			case '[':
			case ']':
			case '(':
			case ')':
			case '>': // Not in markdown documentation, but is in markdown.pl
			case '#':
			case '+':
			case '-':
			case '.':
			case '!':
				return true;

			case ':':
			case '|':
			case '=': // Added for escaping Setext H1
			case '<':
				return extraMode;
		}

		return false;
	}

	// Extension method.  Skip an escapable character, or one normal character
	public static void SkipEscapableChar(this StringScanner p, bool extraMode)
	{
		if (p.Current == '\\' && IsEscapableChar(p.CharAtOffset(1), extraMode))
		{
			p.SkipForward(2);
		}
		else
		{
			p.SkipForward(1);
		}
	}


	// Remove the markdown escapes from a string
	public static string UnescapeString(string str, bool extraMode)
	{
		if (str == null || str.IndexOf('\\') == -1)
			return str;

		var b = new StringBuilder();
		for (var i = 0; i < str.Length; i++)
		{
			if (str[i] == '\\' && i + 1 < str.Length && IsEscapableChar(str[i + 1], extraMode))
			{
				b.Append(str[i + 1]);
				i++;
			}
			else
			{
				b.Append(str[i]);
			}
		}

		return b.ToString();
	}

	// Normalize the line ends in a string to just '\n'
	// Handles all encodings - '\r\n' (windows), '\n\r' (mac), '\n' (unix) '\r' (something?)
	private static readonly char[] lineEnds = { '\r', '\n' };

	public static string NormalizeLineEnds(string str)
	{
		if (str.IndexOfAny(lineEnds) < 0)
			return str;

		var sb = new StringBuilder();
		var sp = new StringScanner(str);
		while (!sp.Eof)
		{
			if (sp.Eol)
			{
				sb.Append('\n');
				sp.SkipEol();
			}
			else
			{
				sb.Append(sp.Current);
				sp.SkipForward(1);
			}
		}

		return sb.ToString();
	}

	/*
	 * These two functions IsEmailAddress and IsWebAddress
	 * are intended as a quick and dirty way to tell if a 
	 * <autolink> url is email, web address or neither.
	 * 
	 * They are not intended as validating checks.
	 * 
	 * (use of Regex for more correct test unnecessarily
	 *  slowed down some test documents by up to 300%.)
	 */

	// Check if a string looks like an email address
	public static bool IsEmailAddress(string str)
	{
		var posAt = str.IndexOf('@');
		if (posAt < 0)
			return false;

		var posLastDot = str.LastIndexOf('.');
		return posLastDot >= posAt;
	}

	// Check if a string looks like a url
	public static bool IsWebAddress(string str)
	{
		return str.StartsWith("http://") ||
				str.StartsWith("https://") ||
				str.StartsWith("ftp://") ||
				str.StartsWith("file://");
	}

	// Check if a string is a valid HTML ID identifier
	private static bool IsValidHtmlID(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;

		// Must start with a letter
		if (!char.IsLetter(str[0]))
			return false;

		// Check the rest
		return str
			.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == ':' || ch == '.');

		// OK
	}

	// Strip the trailing HTML ID from a header string
	// ie:      ## header text ##			{#<idHere>}
	//			^start           ^out end              ^end
	//
	// Returns null if no header id
	public static string StripHtmlID(string str, int start, ref int end)
	{
		// Skip trailing whitespace
		var pos = end - 1;
		while (pos >= start && char.IsWhiteSpace(str[pos]))
		{
			pos--;
		}

		// Skip closing '{'
		if (pos < start || str[pos] != '}')
			return null;

		var endId = pos;
		pos--;

		// Find the opening '{'
		while (pos >= start && str[pos] != '{')
			pos--;

		// Check for the #
		if (pos < start || str[pos + 1] != '#')
			return null;

		// Extract and check the ID
		var startId = pos + 2;
		var strID = str.Substring(startId, endId - startId);
		if (!IsValidHtmlID(strID))
			return null;

		// Skip any preceding whitespace
		while (pos > start && char.IsWhiteSpace(str[pos - 1]))
			pos--;

		// Done!
		end = pos;
		return strID;
	}

	public static bool IsUrlFullyQualified(string url)
	{
		return url.Contains("://") || url.StartsWith("mailto:");
	}
}