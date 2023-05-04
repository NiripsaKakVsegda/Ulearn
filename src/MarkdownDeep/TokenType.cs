namespace MarkdownDeep;

public enum TokenType
{
	Text, // Plain text, should be htmlencoded
	HtmlTag, // Valid html tag, write out directly but escape &amps;
	Html, // Valid html, write out directly
	OpenEm, // <em>
	CloseEm, // </em>
	OpenStrong, // <strong>
	CloseStrong, // </strong>
	CodeSpan, // <code></code>
	Br, // <br/>

	Link, // <a href>, data = LinkInfo
	Img, // <img>, data = LinkInfo
	Footnote, // Footnote reference
	Abbreviation, // An abbreviation, data is a reference to Abbrevation instance

	// These are used during construction of <em> and <strong> tokens
	OpeningMark, // opening '*' or '_'
	ClosingMark, // closing '*' or '_'
	InternalMark, // internal '*' or '_'
}