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

public class SpanFormatter : StringScanner
{
	// Constructor
	// A reference to the owning markdown object is passed in case
	// we need to check for formatting options
	public SpanFormatter(Markdown m)
	{
		markdown = m;
	}


	internal void FormatParagraph(StringBuilder dest, string str, int start, int len)
	{
		// Parse the string into a list of tokens
		Tokenize(str, start, len);

		// Titled image?
		if (allTokens.Count == 1 && markdown.HtmlClassTitledImages != null && allTokens[0].Type == TokenType.Img)
		{
			// Grab the link info
			var li = (LinkInfo) allTokens[0].Data;

			// Render the div opening
			dest.Append("<div class=\"");
			dest.Append(markdown.HtmlClassTitledImages);
			dest.Append("\">\n");

			// Render the img
			markdown.RenderingTitledImage = true;
			Render(dest, str);
			markdown.RenderingTitledImage = false;
			dest.Append('\n');

			// Render the title
			if (!string.IsNullOrEmpty(li.Def.title))
			{
				dest.Append("<p>");
				Utils.SmartHtmlEncodeAmpsAndAngles(dest, li.Def.title);
				dest.Append("</p>\n");
			}

			dest.Append("</div>\n");
		}
		else
		{
			// Render the paragraph
			dest.Append("<p>");
			Render(dest, str);
			dest.Append("</p>\n");
		}
	}

	internal void Format(StringBuilder dest, string str)
	{
		Format(dest, str, 0, str.Length);
	}

	// Format a range in an input string and write it to the destination string builder.
	internal void Format(StringBuilder dest, string str, int start, int len)
	{
		// Parse the string into a list of tokens
		Tokenize(str, start, len);

		// Render all tokens
		Render(dest, str);
	}

	internal void FormatPlain(StringBuilder dest, string str, int start, int len)
	{
		// Parse the string into a list of tokens
		Tokenize(str, start, len);

		// Render all tokens
		RenderPlain(dest, str);
	}

	// Format a string and return it as a new string
	// (used in formatting the text of links)
	public string Format(string str)
	{
		var dest = new StringBuilder();
		Format(dest, str, 0, str.Length);
		return dest.ToString();
	}

	internal string MakeID(string str)
	{
		return MakeID(str, 0, str.Length);
	}

	internal string MakeID(string str, int start, int len)
	{
		// Parse the string into a list of tokens
		Tokenize(str, start, len);

		var sb = new StringBuilder();

		foreach (var t in allTokens)
		{
			switch (t.Type)
			{
				case TokenType.Text:
					sb.Append(str, t.StartOffset, t.Length);
					break;

				case TokenType.Link:
					var li = (LinkInfo) t.Data;
					sb.Append(li.LinkText);
					break;
			}

			FreeToken(t);
		}

		// Now clean it using the same rules as pandoc
		Reset(sb.ToString());

		// Skip everything up to the first letter
		while (!Eof)
		{
			if (char.IsLetter(Current))
				break;
			SkipForward(1);
		}

		// Process all characters
		sb.Length = 0;
		while (!Eof)
		{
			var ch = Current;
			if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.')
				sb.Append(char.ToLower(ch));
			else if (ch == ' ')
				sb.Append('-');
			else if (IsLineEnd(ch))
			{
				sb.Append('-');
				SkipEol();
				continue;
			}

			SkipForward(1);
		}

		return sb.ToString();
	}

	// Render a list of tokens to a destination string builder.
	private void Render(StringBuilder sb, string str)
	{
		foreach (var t in allTokens)
		{
			switch (t.Type)
			{
				case TokenType.Text:
					// Append encoded text
					markdown.HtmlEncode(sb, str, t.StartOffset, t.Length);
					break;

				case TokenType.HtmlTag:
					// Append html as is
					Utils.SmartHtmlEncodeAmps(sb, str, t.StartOffset, t.Length);
					break;

				case TokenType.Html:
				case TokenType.OpeningMark:
				case TokenType.ClosingMark:
				case TokenType.InternalMark:
					// Append html as is
					sb.Append(str, t.StartOffset, t.Length);
					break;

				case TokenType.Br:
					sb.Append("<br />\n");
					break;

				case TokenType.OpenEm:
					sb.Append("<em>");
					break;

				case TokenType.CloseEm:
					sb.Append("</em>");
					break;

				case TokenType.OpenStrong:
					sb.Append("<strong>");
					break;

				case TokenType.CloseStrong:
					sb.Append("</strong>");
					break;

				case TokenType.CodeSpan:
					sb.Append("<code>");
					markdown.HtmlEncode(sb, str, t.StartOffset, t.Length);
					sb.Append("</code>");
					break;

				case TokenType.Link:
				{
					var li = (LinkInfo) t.Data;
					var sf = new SpanFormatter(markdown)
					{
						disableLinks = true
					};

					li.Def.RenderLink(markdown, sb, sf.Format(li.LinkText));
					break;
				}

				case TokenType.Img:
				{
					var li = (LinkInfo) t.Data;
					li.Def.RenderImg(markdown, sb, li.LinkText);
					break;
				}

				case TokenType.Footnote:
				{
					var r = (FootnoteReference) t.Data;
					sb.Append("<sup id=\"fnref:");
					sb.Append(r.Id);
					sb.Append("\"><a href=\"#fn:");
					sb.Append(r.Id);
					sb.Append("\" rel=\"footnote\">");
					sb.Append(r.Index + 1);
					sb.Append("</a></sup>");
					break;
				}

				case TokenType.Abbreviation:
				{
					var a = (Abbreviation) t.Data;
					sb.Append("<abbr");
					if (!string.IsNullOrEmpty(a.Title))
					{
						sb.Append(" title=\"");
						markdown.HtmlEncode(sb, a.Title, 0, a.Title.Length);
						sb.Append('"');
					}

					sb.Append('>');
					markdown.HtmlEncode(sb, a.Abbr, 0, a.Abbr.Length);
					sb.Append("</abbr>");
					break;
				}
			}

			FreeToken(t);
		}
	}

	// Render a list of tokens to a destination string builder.
	private void RenderPlain(StringBuilder sb, string str)
	{
		foreach (var t in allTokens)
		{
			switch (t.Type)
			{
				case TokenType.Text:
					sb.Append(str, t.StartOffset, t.Length);
					break;

				case TokenType.HtmlTag:
					break;

				case TokenType.Html:
				case TokenType.OpeningMark:
				case TokenType.ClosingMark:
				case TokenType.InternalMark:
					break;

				case TokenType.Br:
					break;

				case TokenType.OpenEm:
				case TokenType.CloseEm:
				case TokenType.OpenStrong:
				case TokenType.CloseStrong:
					break;

				case TokenType.CodeSpan:
					sb.Append(str, t.StartOffset, t.Length);
					break;

				case TokenType.Link:
				{
					var li = (LinkInfo) t.Data;
					sb.Append(li.LinkText);
					break;
				}

				case TokenType.Img:
				{
					var li = (LinkInfo) t.Data;
					sb.Append(li.LinkText);
					break;
				}

				case TokenType.Footnote:
				case TokenType.Abbreviation:
					break;
			}

			FreeToken(t);
		}
	}

	// Scan the input string, creating tokens for anything special 
	private void Tokenize(string str, int start, int len)
	{
		// Prepare
		Reset(str, start, len);
		allTokens.Clear();

		List<Token> emphasisMarks = null;

		var abbreviations = markdown.GetAbbreviations();
		var extraMode = markdown.ExtraMode;

		// Scan string
		var startTextToken = Position;
		while (!Eof)
		{
			var endTextToken = Position;

			// Work out token
			Token token = null;
			switch (Current)
			{
				case '*':
				case '_':

					// Create emphasis mark
					token = CreateEmphasisMark();

					if (token != null)
					{
						// Store marks in a separate list the we'll resolve later
						switch (token.Type)
						{
							case TokenType.InternalMark:
							case TokenType.OpeningMark:
							case TokenType.ClosingMark:
								emphasisMarks ??= new List<Token>();

								emphasisMarks.Add(token);
								break;
						}
					}

					break;

				case '`':
					token = ProcessCodeSpan();
					break;

				case '[':
				case '!':
				{
					// Process link reference
					var linkPos = Position;
					token = ProcessLinkOrImageOrFootnote();

					// Rewind if invalid syntax
					// (the '[' or '!' will be treated as a regular character and processed below)
					if (token == null)
						Position = linkPos;
					break;
				}

				case '<':
				{
					// Is it a valid html tag?
					var save = Position;
					var tag = HtmlTag.Parse(this);
					if (tag != null)
					{
						if (!markdown.SafeMode || tag.IsSafe())
						{
							// Yes, create a token for it
							token = CreateToken(TokenType.HtmlTag, save, Position - save);
						}
						else
						{
							// No, rewrite and encode it
							Position = save;
						}
					}
					else
					{
						// No, rewind and check if it's a valid autolink eg: <google.com>
						Position = save;
						token = ProcessAutoLink();

						if (token == null)
							Position = save;
					}

					break;
				}

				case '&':
				{
					// Is it a valid html entity
					var save = Position;
					string unused = null;
					if (SkipHtmlEntity(ref unused))
					{
						// Yes, create a token for it
						token = CreateToken(TokenType.Html, save, Position - save);
					}

					break;
				}

				case ' ':
				{
					// Check for double space at end of a line
					if (CharAtOffset(1) == ' ' && IsLineEnd(CharAtOffset(2)))
					{
						// Yes, skip it
						SkipForward(2);

						// Don't put br's at the end of a paragraph
						if (!Eof)
						{
							SkipEol();
							token = CreateToken(TokenType.Br, endTextToken, 0);
						}
					}

					break;
				}

				case '\\':
				{
					// Check followed by an escapable character
					if (Utils.IsEscapableChar(CharAtOffset(1), extraMode))
					{
						token = CreateToken(TokenType.Text, Position + 1, 1);
						SkipForward(2);
					}

					break;
				}
			}

			// Look for abbreviations.
			if (token == null && abbreviations != null && !char.IsLetterOrDigit(CharAtOffset(-1)))
			{
				var savePos = Position;
				foreach (var abbr in abbreviations)
				{
					if (SkipString(abbr.Abbr) && !char.IsLetterOrDigit(Current))
					{
						token = CreateToken(TokenType.Abbreviation, abbr);
						break;
					}

					Position = savePos;
				}
			}

			// If token found, append any preceding text and the new token to the token list
			if (token != null)
			{
				// Create a token for everything up to the special character
				if (endTextToken > startTextToken)
				{
					allTokens.Add(CreateToken(TokenType.Text, startTextToken, endTextToken - startTextToken));
				}

				// Add the new token
				allTokens.Add(token);

				// Remember where the next text token starts
				startTextToken = Position;
			}
			else
			{
				// Skip a single character and keep looking
				SkipForward(1);
			}
		}

		// Append a token for any trailing text after the last token.
		if (Position > startTextToken)
		{
			allTokens.Add(CreateToken(TokenType.Text, startTextToken, Position - startTextToken));
		}

		// Do we need to resolve and emphasis marks?
		if (emphasisMarks != null)
		{
			ResolveEmphasisMarks(allTokens, emphasisMarks);
		}

		// Done!
	}

	private static bool IsEmphasisChar(char ch)
	{
		return ch is '_' or '*';
	}

	/*
	 * Resolving emphasis tokens is a two part process
	 * 
	 * 1. Find all valid sequences of * and _ and create `mark` tokens for them
	 *		this is done by CreateEmphasisMarks during the initial character scan
	 *		done by Tokenize
	 *		
	 * 2. Looks at all these emphasis marks and tries to pair them up
	 *		to make the actual <em> and <strong> tokens
	 *		
	 * Any unresolved emphasis marks are rendered unaltered as * or _
	 */

	// Create emphasis mark for sequences of '*' and '_' (part 1)
	private Token CreateEmphasisMark()
	{
		// Capture current state
		var ch = Current;
		var savePos = Position;

		// Check for a consecutive sequence of just '_' and '*'
		if (Bof || char.IsWhiteSpace(CharAtOffset(-1)))
		{
			while (IsEmphasisChar(Current))
				SkipForward(1);

			if (Eof || char.IsWhiteSpace(Current))
			{
				return new Token(TokenType.Html, savePos, Position - savePos);
			}

			// Rewind
			Position = savePos;
		}

		// Scan backwards and see if we have space before
		while (IsEmphasisChar(CharAtOffset(-1)))
			SkipForward(-1);
		var bSpaceBefore = Bof || char.IsWhiteSpace(CharAtOffset(-1));
		Position = savePos;

		// Count how many matching emphasis characters
		while (Current == ch)
		{
			SkipForward(1);
		}

		var count = Position - savePos;

		// Scan forwards and see if we have space after
		while (IsEmphasisChar(CharAtOffset(1)))
			SkipForward(1);
		var bSpaceAfter = Eof || char.IsWhiteSpace(Current);
		Position = savePos + count;

		// This should have been stopped by check above
		System.Diagnostics.Debug.Assert(!bSpaceBefore || !bSpaceAfter);

		if (bSpaceBefore)
		{
			return CreateToken(TokenType.OpeningMark, savePos, Position - savePos);
		}

		if (bSpaceAfter)
		{
			return CreateToken(TokenType.ClosingMark, savePos, Position - savePos);
		}

		if (markdown.ExtraMode && ch == '_' && (char.IsLetterOrDigit(Current)))
			return null;

		return CreateToken(TokenType.InternalMark, savePos, Position - savePos);
	}

	// Split mark token
	private Token SplitMarkToken(IList<Token> tokens, IList<Token> marks, Token token, int pos)
	{
		// Create the new rhs token
		var tokenRhs = CreateToken(token.Type, token.StartOffset + pos, token.Length - pos);

		// Adjust down the length of this token
		token.Length = pos;

		// Insert the new token into each of the parent collections
		marks.Insert(marks.IndexOf(token) + 1, tokenRhs);
		tokens.Insert(tokens.IndexOf(token) + 1, tokenRhs);

		// Return the new token
		return tokenRhs;
	}

	// Resolve emphasis marks (part 2)
	private void ResolveEmphasisMarks(IList<Token> tokens, IList<Token> marks)
	{
		var bContinue = true;
		while (bContinue)
		{
			bContinue = false;
			for (var i = 0; i < marks.Count; i++)
			{
				// Get the next opening or internal mark
				var openingMark = marks[i];
				if (openingMark.Type != TokenType.OpeningMark && openingMark.Type != TokenType.InternalMark)
					continue;

				// Look for a matching closing mark
				for (var j = i + 1; j < marks.Count; j++)
				{
					// Get the next closing or internal mark
					var closingMark = marks[j];
					if (closingMark.Type != TokenType.ClosingMark && closingMark.Type != TokenType.InternalMark)
						break;

					// Ignore if different type (ie: `*` vs `_`)
					if (Input[openingMark.StartOffset] != Input[closingMark.StartOffset])
						continue;

					// strong or em?
					var style = Math.Min(openingMark.Length, closingMark.Length);

					// Triple or more on both ends?
					if (style >= 3)
					{
						style = (style % 2) == 1 ? 1 : 2;
					}

					// Split the opening mark, keeping the RHS
					if (openingMark.Length > style)
					{
						openingMark = SplitMarkToken(tokens, marks, openingMark, openingMark.Length - style);
						i--;
					}

					// Split the closing mark, keeping the LHS
					if (closingMark.Length > style)
					{
						SplitMarkToken(tokens, marks, closingMark, style);
					}

					// Connect them
					openingMark.Type = style == 1 ? TokenType.OpenEm : TokenType.OpenStrong;
					closingMark.Type = style == 1 ? TokenType.CloseEm : TokenType.CloseStrong;

					// Remove the matched marks
					marks.Remove(openingMark);
					marks.Remove(closingMark);
					bContinue = true;

					break;
				}
			}
		}
	}

	// Resolve emphasis marks (part 2)
	public void ResolveEmphasisMarks_classic(List<Token> tokens, List<Token> marks)
	{
		// First pass, do <strong>
		for (var i = 0; i < marks.Count; i++)
		{
			// Get the next opening or internal mark
			var openingMark = marks[i];
			if (openingMark.Type != TokenType.OpeningMark && openingMark.Type != TokenType.InternalMark)
				continue;
			if (openingMark.Length < 2)
				continue;

			// Look for a matching closing mark
			for (var j = i + 1; j < marks.Count; j++)
			{
				// Get the next closing or internal mark
				var closingMark = marks[j];
				if (closingMark.Type != TokenType.ClosingMark && closingMark.Type != TokenType.InternalMark)
					continue;

				// Ignore if different type (ie: `*` vs `_`)
				if (Input[openingMark.StartOffset] != Input[closingMark.StartOffset])
					continue;

				// Must be at least two
				if (closingMark.Length < 2)
					continue;

				// Split the opening mark, keeping the LHS
				if (openingMark.Length > 2)
				{
					SplitMarkToken(tokens, marks, openingMark, 2);
				}

				// Split the closing mark, keeping the RHS
				if (closingMark.Length > 2)
				{
					closingMark = SplitMarkToken(tokens, marks, closingMark, closingMark.Length - 2);
				}

				// Connect them
				openingMark.Type = TokenType.OpenStrong;
				closingMark.Type = TokenType.CloseStrong;

				// Continue after the closing mark
				i = marks.IndexOf(closingMark);
				break;
			}
		}

		// Second pass, do <em>
		for (var i = 0; i < marks.Count; i++)
		{
			// Get the next opening or internal mark
			var openingMark = marks[i];
			if (openingMark.Type != TokenType.OpeningMark && openingMark.Type != TokenType.InternalMark)
				continue;

			// Look for a matching closing mark
			for (var j = i + 1; j < marks.Count; j++)
			{
				// Get the next closing or internal mark
				var closingMark = marks[j];
				if (closingMark.Type != TokenType.ClosingMark && closingMark.Type != TokenType.InternalMark)
					continue;

				// Ignore if different type (ie: `*` vs `_`)
				if (Input[openingMark.StartOffset] != Input[closingMark.StartOffset])
					continue;

				// Split the opening mark, keeping the LHS
				if (openingMark.Length > 1)
				{
					SplitMarkToken(tokens, marks, openingMark, 1);
				}

				// Split the closing mark, keeping the RHS
				if (closingMark.Length > 1)
				{
					closingMark = SplitMarkToken(tokens, marks, closingMark, closingMark.Length - 1);
				}

				// Connect them
				openingMark.Type = TokenType.OpenEm;
				closingMark.Type = TokenType.CloseEm;

				// Continue after the closing mark
				i = marks.IndexOf(closingMark);
				break;
			}
		}
	}

	// Process '*', '**' or '_', '__'
	// This is horrible and probably much better done through regex, but I'm stubborn.
	// For normal cases this routine works as expected.  For unusual cases (eg: overlapped
	// strong and emphasis blocks), the behaviour is probably not the same as the original
	// markdown scanner.
	/*
	public Token ProcessEmphasisOld(ref Token prev_single, ref Token prev_double)
	{
		// Check whitespace before/after
		bool bSpaceBefore = !bof && IsLineSpace(CharAtOffset(-1));
		bool bSpaceAfter = IsLineSpace(CharAtOffset(1));

		// Ignore if surrounded by whitespace
		if (bSpaceBefore && bSpaceAfter)
		{
			return null;
		}

		// Save the current character and skip it
		char ch = current;
		Skip(1);

		// Do we have a previous matching single star?
		if (!bSpaceBefore && prev_single != null)
		{
			// Yes, match them...
			prev_single.type = TokenType.open_em;
			prev_single = null;
			return CreateToken(TokenType.close_em, position - 1, 1);
		}

		// Is this a double star/under
		if (current == ch)
		{
			// Skip second character
			Skip(1);

			// Space after?
			bSpaceAfter = IsLineSpace(current);

			// Space both sides?
			if (bSpaceBefore && bSpaceAfter)
			{
				// Ignore it
				return CreateToken(TokenType.Text, position - 2, 2);
			}

			// Do we have a previous matching double
			if (!bSpaceBefore && prev_double != null)
			{
				// Yes, match them
				prev_double.type = TokenType.open_strong;
				prev_double = null;
				return CreateToken(TokenType.close_strong, position - 2, 2);
			}

			if (!bSpaceAfter)
			{
				// Opening double star
				prev_double = CreateToken(TokenType.Text, position - 2, 2);
				return prev_double;
			}

			// Ignore it
			return CreateToken(TokenType.Text, position - 2, 2);
		}

		// If there's a space before, we can open em
		if (!bSpaceAfter)
		{
			// Opening single star
			prev_single = CreateToken(TokenType.Text, position - 1, 1);
			return prev_single;
		}

		// Ignore
		Skip(-1);
		return null;
	}
	 */

	// Process auto links eg: <google.com>
	private Token ProcessAutoLink()
	{
		if (disableLinks)
			return null;

		// Skip the angle bracket and remember the start
		SkipForward(1);
		Mark();

		var extraMode = markdown.ExtraMode;

		// Allow anything up to the closing angle, watch for escapable characters
		while (!Eof)
		{
			var ch = Current;

			// No whitespace allowed
			if (char.IsWhiteSpace(ch))
				break;

			// End found?
			if (ch == '>')
			{
				var url = Utils.UnescapeString(Extract(), extraMode);

				LinkInfo li = null;
				if (Utils.IsEmailAddress(url))
				{
					string linkText;
					if (url.StartsWith("mailto:"))
					{
						linkText = url[7..];
					}
					else
					{
						linkText = url;
						url = "mailto:" + url;
					}

					li = new LinkInfo(new LinkDefinition("auto", url, null), linkText);
				}
				else if (Utils.IsWebAddress(url))
				{
					li = new LinkInfo(new LinkDefinition("auto", url, null), url);
				}

				if (li != null)
				{
					SkipForward(1);
					return CreateToken(TokenType.Link, li);
				}

				return null;
			}

			this.SkipEscapableChar(extraMode);
		}

		// Didn't work
		return null;
	}

	// Process [link] and ![image] directives
	private Token ProcessLinkOrImageOrFootnote()
	{
		// Link or image?
		var tokenType = SkipChar('!') ? TokenType.Img : TokenType.Link;

		// Opening '['
		if (!SkipChar('['))
			return null;

		// Is it a footnote?
		var savePos = Position;
		if (markdown.ExtraMode && tokenType == TokenType.Link && SkipChar('^'))
		{
			SkipLineSpace();

			// Parse it
			if (SkipFootnoteID(out var id) && SkipChar(']'))
			{
				// Look it up and create footnote reference token
				var footnoteIndex = markdown.ClaimFootnote(id);
				if (footnoteIndex >= 0)
				{
					// Yes it's a footnote
					return CreateToken(TokenType.Footnote, new FootnoteReference(footnoteIndex, id));
				}
			}

			// Rewind
			Position = savePos;
		}

		if (disableLinks && tokenType == TokenType.Link)
			return null;

		var extraMode = markdown.ExtraMode;

		// Find the closing square bracket, allowing for nesting, watching for 
		// escapable characters
		Mark();
		var depth = 1;
		while (!Eof)
		{
			var ch = Current;
			if (ch == '[')
			{
				depth++;
			}
			else if (ch == ']')
			{
				depth--;
				if (depth == 0)
					break;
			}

			this.SkipEscapableChar(extraMode);
		}

		// Quit if end
		if (Eof)
			return null;

		// Get the link text and unescape it
		var linkText = Utils.UnescapeString(Extract(), extraMode);

		// The closing ']'
		SkipForward(1);

		// Save position in case we need to rewind
		savePos = Position;

		// Inline links must follow immediately
		if (SkipChar('('))
		{
			// Extract the url and title
			var linkDef = LinkDefinition.ParseLinkTarget(this, null, markdown.ExtraMode);
			if (linkDef == null)
				return null;

			// Closing ')'
			SkipWhitespace();
			if (!SkipChar(')'))
				return null;

			// Create the token
			return CreateToken(tokenType, new LinkInfo(linkDef, linkText));
		}

		// Optional space or tab
		if (!SkipChar(' '))
			SkipChar('\t');

		// If there's line end, we're allow it and as must line space as we want
		// before the link id.
		if (Eol)
		{
			SkipEol();
			SkipLineSpace();
		}

		// Reference link?
		string linkID = null;
		if (Current == '[')
		{
			// Skip the opening '['
			SkipForward(1);

			// Find the start/end of the id
			Mark();
			if (!Find(']'))
				return null;

			// Extract the id
			linkID = Extract();

			// Skip closing ']'
			SkipForward(1);
		}
		else
		{
			// Rewind to just after the closing ']'
			Position = savePos;
		}

		// Link id not specified?
		if (string.IsNullOrEmpty(linkID))
		{
			// Use the link text (implicit reference link)
			linkID = Utils.NormalizeLineEnds(linkText);

			// If the link text has carriage returns, normalize
			// to spaces
			if (!ReferenceEquals(linkID, linkText))
			{
				while (linkID.Contains(" \n"))
					linkID = linkID.Replace(" \n", "\n");
				linkID = linkID.Replace("\n", " ");
			}
		}

		// Find the link definition abort if not defined
		var def = markdown.GetLinkDefinition(linkID);
		return def == null 
			? null
			: CreateToken(tokenType, new LinkInfo(def, linkText));
	}

	// Process a ``` code span ```
	private Token ProcessCodeSpan()
	{
		var start = Position;

		// Count leading ticks
		var tickCount = 0;
		while (SkipChar('`'))
		{
			tickCount++;
		}

		// Skip optional leading space...
		SkipWhitespace();

		// End?
		if (Eof)
			return CreateToken(TokenType.Text, start, Position - start);

		var startOfCode = Position;

		// Find closing ticks
		if (!Find(Substring(start, tickCount)))
			return CreateToken(TokenType.Text, start, Position - start);

		// Save end position before backing up over trailing whitespace
		var endPos = Position + tickCount;
		while (char.IsWhiteSpace(CharAtOffset(-1)))
			SkipForward(-1);

		// Create the token, move back to the end and we're done
		var ret = CreateToken(TokenType.CodeSpan, startOfCode, Position - startOfCode);
		Position = endPos;
		return ret;
	}


	#region Token Pooling

	// CreateToken - create or re-use a token object
	private Token CreateToken(TokenType type, int startOffset, int length)
	{
		if (spareTokens.Count == 0)
			return new Token(type, startOffset, length);

		var t = spareTokens.Pop();
		t.Type = type;
		t.StartOffset = startOffset;
		t.Length = length;
		t.Data = null;
		return t;
	}

	// CreateToken - create or re-use a token object
	private Token CreateToken(TokenType type, object data)
	{
		if (spareTokens.Count != 0)
		{
			var t = spareTokens.Pop();
			t.Type = type;
			t.Data = data;
			return t;
		}
		else
			return new Token(type, data);
	}

	// FreeToken - return a token to the spare token pool
	private void FreeToken(Token token)
	{
		token.Data = null;
		spareTokens.Push(token);
	}

	private readonly Stack<Token> spareTokens = new();

	#endregion

	private readonly Markdown markdown;
	private bool disableLinks;
	private readonly List<Token> allTokens = new();
}