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

public class Block
{
	public BlockType BlockType;
	internal List<Block> Children;
	internal string Buf;
	internal int ContentStart;
	internal int ContentLen;
	internal int LineStart;
	internal int LineLen;
	internal object Data; // content depends on block type

	internal Block()
	{
	}

	internal Block(BlockType type)
	{
		BlockType = type;
	}

	public string Content
	{
		get
		{
			if (BlockType == BlockType.codeblock)
			{
				var s = new StringBuilder();
				foreach (var line in Children)
				{
					s.Append(line.Content);
					s.Append('\n');
				}

				return s.ToString();
			}


			if (Buf == null)
				return null;

			return ContentStart == -1 ? Buf : Buf.Substring(ContentStart, ContentLen);
		}
	}

	internal void RenderChildren(Markdown m, StringBuilder b)
	{
		foreach (var block in Children)
		{
			block.Render(m, b);
		}
	}

	internal void RenderChildrenPlain(Markdown m, StringBuilder b)
	{
		foreach (var block in Children)
		{
			block.RenderPlain(m, b);
		}
	}

	internal string ResolveHeaderID(Markdown m)
	{
		// Already resolved?
		if (Data is string data)
			return data;

		// Approach 1 - PHP Markdown Extra style header id
		var end = ContentEnd;
		var id = Utils.StripHtmlID(Buf, ContentStart, ref end);
		if (id != null)
		{
			ContentEnd = end;
		}
		else
		{
			// Approach 2 - pandoc style header id
			id = m.MakeUniqueHeaderID(Buf, ContentStart, ContentLen);
		}

		Data = id;
		return id;
	}

	internal void Render(Markdown m, StringBuilder b)
	{
		switch (BlockType)
		{
			case BlockType.Blank:
				return;

			case BlockType.p:
				m.SpanFormatter.FormatParagraph(b, Buf, ContentStart, ContentLen);
				break;

			case BlockType.span:
				m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
				b.Append('\n');
				break;

			case BlockType.h1:
			case BlockType.h2:
			case BlockType.h3:
			case BlockType.h4:
			case BlockType.h5:
			case BlockType.h6:
				if (m.ExtraMode && !m.SafeMode)
				{
					b.Append("<" + BlockType.ToString());
					var id = ResolveHeaderID(m);
					if (!string.IsNullOrEmpty(id))
					{
						b.Append(" id=\"");
						b.Append(id);
						b.Append("\">");
					}
					else
					{
						b.Append('>');
					}
				}
				else
				{
					b.Append("<" + BlockType.ToString() + ">");
				}

				m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
				b.Append("</" + BlockType.ToString() + ">\n");
				break;

			case BlockType.hr:
				b.Append("<hr />\n");
				return;

			case BlockType.user_break:
				return;

			case BlockType.ol_li:
			case BlockType.ul_li:
				b.Append("<li>");
				m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
				b.Append("</li>\n");
				break;

			case BlockType.dd:
				b.Append("<dd>");
				if (Children != null)
				{
					b.Append('\n');
					RenderChildren(m, b);
				}
				else
					m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);

				b.Append("</dd>\n");
				break;

			case BlockType.dt:
			{
				if (Children == null)
				{
					foreach (var l in Content.Split('\n'))
					{
						b.Append("<dt>");
						m.SpanFormatter.Format(b, l.Trim());
						b.Append("</dt>\n");
					}
				}
				else
				{
					b.Append("<dt>\n");
					RenderChildren(m, b);
					b.Append("</dt>\n");
				}

				break;
			}

			case BlockType.dl:
				b.Append("<dl>\n");
				RenderChildren(m, b);
				b.Append("</dl>\n");
				return;

			case BlockType.html:
				b.Append(Buf, ContentStart, ContentLen);
				return;

			case BlockType.unsafe_html:
				m.HtmlEncode(b, Buf, ContentStart, ContentLen);
				return;

			case BlockType.codeblock:
				if (m.FormatCodeBlock != null)
				{
					var sb = new StringBuilder();
					foreach (var line in Children)
					{
						m.HtmlEncodeAndConvertTabsToSpaces(sb, line.Buf, line.ContentStart, line.ContentLen);
						sb.Append('\n');
					}

					b.Append(m.FormatCodeBlock(m, sb.ToString(), ""));
				}
				else
				{
					b.Append("<pre><code>");
					foreach (var line in Children)
					{
						m.HtmlEncodeAndConvertTabsToSpaces(b, line.Buf, line.ContentStart, line.ContentLen);
						b.Append('\n');
					}

					b.Append("</code></pre>\n\n");
				}

				return;

			case BlockType.codeblockWithLang:
				var lang = Children[0].Content.Trim();
				var code = new StringBuilder();
				m.HtmlEncodeAndConvertTabsToSpaces(
					code,
					Children[1].Buf,
					Children[1].ContentStart,
					Children[1].ContentLen
				);

				if (m.FormatCodeBlock != null)
				{
					b.Append(m.FormatCodeBlock(m, code.ToString(), lang));
				}
				else
				{
					b.Append(string.IsNullOrWhiteSpace(lang) ? "<pre><code>" : $"<pre><code data-lang=\"{lang}\">")
						.Append(code)
						.AppendLine()
						.Append("</code></pre>\n\n");
				}

				return;

			case BlockType.quote:
				b.Append("<blockquote>\n");
				RenderChildren(m, b);
				b.Append("</blockquote>\n");
				return;

			case BlockType.li:
				b.Append("<li>\n");
				RenderChildren(m, b);
				b.Append("</li>\n");
				return;

			case BlockType.ol:
				b.Append("<ol>\n");
				RenderChildren(m, b);
				b.Append("</ol>\n");
				return;

			case BlockType.ul:
				b.Append("<ul>\n");
				RenderChildren(m, b);
				b.Append("</ul>\n");
				return;

			case BlockType.HtmlTag:
				var tag = (HtmlTag) Data;

				// Prepare special tags
				var name = tag.Name.ToLowerInvariant();
				if (name == "a")
				{
					m.OnPrepareLink(tag);
				}
				else if (name == "img")
				{
					m.OnPrepareImage(tag, m.RenderingTitledImage);
				}

				tag.RenderOpening(b);
				b.Append('\n');
				RenderChildren(m, b);
				tag.RenderClosing(b);
				b.Append('\n');
				return;

			case BlockType.Composite:
			case BlockType.footnote:
				RenderChildren(m, b);
				return;

			case BlockType.table_spec:
				((TableSpec) Data).Render(m, b);
				break;

			case BlockType.p_footnote:
				b.Append("<p>");
				if (ContentLen > 0)
				{
					m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
					b.Append("&nbsp;");
				}

				b.Append((string) Data);
				b.Append("</p>\n");
				break;

			default:
				b.Append("<" + BlockType + ">");
				m.SpanFormatter.Format(b, Buf, ContentStart, ContentLen);
				b.Append("</" + BlockType.ToString() + ">\n");
				break;
		}
	}

	internal void RenderPlain(Markdown m, StringBuilder b)
	{
		switch (BlockType)
		{
			case BlockType.Blank:
				return;

			case BlockType.p:
			case BlockType.span:
				m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
				b.Append(' ');
				break;

			case BlockType.h1:
			case BlockType.h2:
			case BlockType.h3:
			case BlockType.h4:
			case BlockType.h5:
			case BlockType.h6:
				m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
				b.Append(" - ");
				break;


			case BlockType.ol_li:
			case BlockType.ul_li:
				b.Append("* ");
				m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);
				b.Append(' ');
				break;

			case BlockType.dd:
				if (Children != null)
				{
					b.Append('\n');
					RenderChildrenPlain(m, b);
				}
				else
					m.SpanFormatter.FormatPlain(b, Buf, ContentStart, ContentLen);

				break;

			case BlockType.dt:
			{
				if (Children == null)
				{
					foreach (var l in Content.Split('\n'))
					{
						var str = l.Trim();
						m.SpanFormatter.FormatPlain(b, str, 0, str.Length);
					}
				}
				else
				{
					RenderChildrenPlain(m, b);
				}

				break;
			}

			case BlockType.dl:
				RenderChildrenPlain(m, b);
				return;

			case BlockType.codeblock:
				foreach (var line in Children)
				{
					b.Append(line.Buf, line.ContentStart, line.ContentLen);
					b.Append(' ');
				}

				return;

			case BlockType.quote:
			case BlockType.li:
			case BlockType.ol:
			case BlockType.ul:
			case BlockType.HtmlTag:
				RenderChildrenPlain(m, b);
				return;
		}
	}

	public void RevertToPlain()
	{
		BlockType = BlockType.p;
		ContentStart = LineStart;
		ContentLen = LineLen;
	}

	public int ContentEnd
	{
		get => ContentStart + ContentLen;
		set => ContentLen = value - ContentStart;
	}

	// Count the leading spaces on a block
	// Used by list item evaluation to determine indent levels
	// irrespective of indent line type.
	public int LeadingSpaces
	{
		get
		{
			var count = 0;
			for (var i = LineStart; i < LineStart + LineLen; i++)
			{
				if (Buf[i] == ' ')
				{
					count++;
				}
				else
				{
					break;
				}
			}

			return count;
		}
	}

	public override string ToString()
	{
		var c = Content;
		return BlockType + " - " + (c ?? "<null>");
	}

	public Block CopyFrom(Block other)
	{
		BlockType = other.BlockType;
		Buf = other.Buf;
		ContentStart = other.ContentStart;
		ContentLen = other.ContentLen;
		LineStart = other.LineStart;
		LineLen = other.LineLen;
		return this;
	}
}