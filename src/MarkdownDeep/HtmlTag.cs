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

[Flags]
public enum HtmlTagFlags
{
	Block = 0x0001, // Block tag
	Inline = 0x0002, // Inline tag
	NoClosing = 0x0004, // No closing tag (eg: <hr> and <!-- -->)
	ContentAsSpan = 0x0008, // When markdown=1 treat content as span, not block
};

public class HtmlTag
{
	public HtmlTag(string name)
	{
		Name = name;
	}

	// Get the tag name eg: "div"
	public string Name { get; }

	// Get a dictionary of attribute values (no decoding done)
	public Dictionary<string, string> Attributes { get; } = new(StringComparer.CurrentCultureIgnoreCase);

	// Is this tag closed eg; <br />
	public bool Closed { get; set; }

	// Is this a closing tag eg: </div>
	public bool Closing { get; private set; }

	private HtmlTagFlags flags = 0;

	public HtmlTagFlags Flags
	{
		get
		{
			if (flags == 0 && !tagFlags.TryGetValue(Name.ToLower(), out flags))
			{
				flags |= HtmlTagFlags.Inline;
			}

			return flags;
		}
	}

	private static readonly string[] allowedTags =
	{
		"b", "blockquote", "code", "dd", "dt", "dl", "del", "em", "h1", "h2", "h3", "h4", "h5", "h6", "i", "kbd", "li",
		"ol", "ul",
		"p", "pre", "s", "sub", "sup", "strong", "strike", "img", "a"
	};

	private static readonly Dictionary<string, string[]> allowedAttributes = new()
	{
		{ "a", new[] { "href", "title", "class" } },
		{ "img", new[] { "src", "width", "height", "alt", "title", "class" } },
	};

	private static readonly Dictionary<string, HtmlTagFlags> tagFlags = new()
	{
		{ "p", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "div", HtmlTagFlags.Block },
		{ "h1", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "h2", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "h3", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "h4", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "h5", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "h6", HtmlTagFlags.Block | HtmlTagFlags.ContentAsSpan },
		{ "blockquote", HtmlTagFlags.Block },
		{ "pre", HtmlTagFlags.Block },
		{ "table", HtmlTagFlags.Block },
		{ "dl", HtmlTagFlags.Block },
		{ "ol", HtmlTagFlags.Block },
		{ "ul", HtmlTagFlags.Block },
		{ "form", HtmlTagFlags.Block },
		{ "fieldset", HtmlTagFlags.Block },
		{ "iframe", HtmlTagFlags.Block },
		{ "script", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "noscript", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "math", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "ins", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "del", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "img", HtmlTagFlags.Block | HtmlTagFlags.Inline },
		{ "li", HtmlTagFlags.ContentAsSpan },
		{ "dd", HtmlTagFlags.ContentAsSpan },
		{ "dt", HtmlTagFlags.ContentAsSpan },
		{ "td", HtmlTagFlags.ContentAsSpan },
		{ "th", HtmlTagFlags.ContentAsSpan },
		{ "legend", HtmlTagFlags.ContentAsSpan },
		{ "address", HtmlTagFlags.ContentAsSpan },
		{ "hr", HtmlTagFlags.Block | HtmlTagFlags.NoClosing },
		{ "!", HtmlTagFlags.Block | HtmlTagFlags.NoClosing },
		{ "head", HtmlTagFlags.Block },
	};

	// Check if this tag is safe
	public bool IsSafe()
	{
		var nameLower = Name.ToLowerInvariant();

		// Check if tag is in whitelist
		if (!Utils.IsInList(nameLower, allowedTags))
			return false;

		// Find allowed attributes
		if (!allowedAttributes.TryGetValue(nameLower, out var tagAllowedAttributes))
		{
			// No allowed attributes, check we don't have any
			return Attributes.Count == 0;
		}

		// Check all are allowed
		if (Attributes.Any(i => !Utils.IsInList(i.Key.ToLowerInvariant(), tagAllowedAttributes)))
			return false;

		// Check href attribute is ok
		if (Attributes.TryGetValue("href", out var href))
		{
			if (!Utils.IsSafeUrl(href))
				return false;
		}

		return !Attributes.TryGetValue("src", out var src) || Utils.IsSafeUrl(src);
		// Passed all white list checks, allow it
	}

	// Render opening tag (eg: <tag attr="value">
	public void RenderOpening(StringBuilder dest)
	{
		dest.Append('<')
			.Append(Name);
		foreach (var i in Attributes)
		{
			dest.Append(' ')
				.Append(i.Key)
				.Append("=\"")
				.Append(i.Value)
				.Append('"');
		}

		dest.Append(Closed ? " />" : ">");
	}

	// Render closing tag (eg: </tag>)
	public void RenderClosing(StringBuilder dest)
	{
		dest.Append("</")
			.Append(Name)
			.Append('>');
	}


	public static HtmlTag Parse(string str, ref int pos)
	{
		var sp = new StringScanner(str, pos);
		var ret = Parse(sp);

		if (ret == null)
			return null;

		pos = sp.Position;
		return ret;
	}

	public static HtmlTag Parse(StringScanner p)
	{
		// Save position
		var savePos = p.Position;

		// Parse it
		var ret = ParseHelper(p);
		if (ret != null)
			return ret;

		// Rewind if failed
		p.Position = savePos;
		return null;
	}

	private static HtmlTag ParseHelper(StringScanner p)
	{
		// Does it look like a tag?
		if (p.Current != '<')
			return null;

		// Skip '<'
		p.SkipForward(1);

		// Is it a comment?
		if (p.SkipString("!--"))
		{
			p.Mark();

			if (p.Find("-->"))
			{
				var t = new HtmlTag("!");
				t.Attributes.Add("content", p.Extract());
				t.Closed = true;
				p.SkipForward(3);
				return t;
			}
		}

		// Is it a closing tag eg: </div>
		var bClosing = p.SkipChar('/');

		// Get the tag name
		string tagName = null;
		if (!p.SkipIdentifier(ref tagName))
			return null;

		// Probably a tag, create the HtmlTag object now
		var tag = new HtmlTag(tagName)
		{
			Closing = bClosing
		};

		// If it's a closing tag, no attributes
		if (bClosing)
		{
			if (p.Current != '>')
				return null;

			p.SkipForward(1);
			return tag;
		}

		while (!p.Eof)
		{
			// Skip whitespace
			p.SkipWhitespace();

			// Check for closed tag eg: <hr />
			if (p.SkipString("/>"))
			{
				tag.Closed = true;
				return tag;
			}

			// End of tag?
			if (p.SkipChar('>'))
			{
				return tag;
			}

			// attribute name
			string attributeName = null;
			if (!p.SkipIdentifier(ref attributeName))
				return null;

			// Skip whitespace
			p.SkipWhitespace();

			// Skip equal sign
			if (p.SkipChar('='))
			{
				// Skip whitespace
				p.SkipWhitespace();

				// Optional quotes
				if (p.SkipChar('\"'))
				{
					// Scan the value
					p.Mark();
					if (!p.Find('\"'))
						return null;

					// Store the value
					tag.Attributes.Add(attributeName, p.Extract());

					// Skip closing quote
					p.SkipForward(1);
				}
				else
				{
					// Scan the value
					p.Mark();
					while (!p.Eof && !char.IsWhiteSpace(p.Current) && p.Current != '>' && p.Current != '/')
						p.SkipForward(1);

					if (!p.Eof)
					{
						// Store the value
						tag.Attributes.Add(attributeName, p.Extract());
					}
				}
			}
			else
			{
				tag.Attributes.Add(attributeName, "");
			}
		}

		return null;
	}
}