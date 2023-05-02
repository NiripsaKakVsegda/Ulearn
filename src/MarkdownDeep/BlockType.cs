namespace MarkdownDeep;

public enum BlockType
{
	Blank,			// blank line (parse only)
	h1,				// headings (render and parse)
	h2, 
	h3, 
	h4, 
	h5, 
	h6,
	post_h1,		// setext heading lines (parse only)
	post_h2,
	quote,			// block quote (render and parse)
	ol_li,			// list item in an ordered list	(render and parse)
	ul_li,			// list item in an unordered list (render and parse)
	p,				// paragraph (or plain line during parse)
	indent,			// an indented line (parse only)
	hr,				// horizontal rule (render and parse)
	user_break,		// user break
	html,			// html content (render and parse)
	unsafe_html,	// unsafe html that should be encoded
	span,			// an undecorated span of text (used for simple list items 
	//			where content is not wrapped in paragraph tags
	codeblock,		// a code block (render only)
	codeblockWithLang,		// a code block (render only)
	li,				// a list item (render only)
	ol,				// ordered list (render only)
	ul,				// unordered list (render only)
	HtmlTag,		// Data=(HtmlTag), children = content
	Composite,		// Just a list of child blocks
	table_spec,		// A table row specifier eg:  |---: | ---|	`data` = TableSpec reference
	dd,				// definition (render and parse)	`data` = bool true if blank line before
	dt,				// render only
	dl,				// render only
	footnote,		// footnote definition  eg: [^id]   `data` holds the footnote id
	p_footnote,		// paragraph with footnote return link append.  Return link string is in `data`.
}