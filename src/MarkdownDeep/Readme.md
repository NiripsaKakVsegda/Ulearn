# MarkdownDeep

## Original library:

- [Official Website](https://www.toptensoftware.com/markdowndeep)
- [GitHub](https://github.com/toptensoftware/MarkdownDeep)

## Changes:

### Code block language specifying support

Added support for specifying code block language.

Next code:

```markdown
	```csharp
		Console.WriteLine("Hello World!");
	```
```

Converts to:

```html
	<pre><code data-lang="csharp">
		Console.WriteLine("Hello World!");
	</code></pre>
```

Also you can format code block by your own using ```Markdown.FormatCodeBlock``` delegate:
```csharp
var md = new Markdown();
md.FormatCodeBlock += (md, code, lang) => {
	return $"<textarea class='code code-sample' data-lang='{lang}'>{code}</textarea>\n"
}
```