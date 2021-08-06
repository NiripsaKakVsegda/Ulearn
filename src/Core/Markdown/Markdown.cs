using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using MarkdownDeep;
using Microsoft.AspNetCore.Html;
using Ulearn.Core.Courses;

namespace Ulearn.Core.Markdown
{
	public record MarkdownRenderContext(string BaseUrlApi, string BaseUrlWeb, string CourseId, string UnitDirectoryRelativeToCourse);

	public static class Markdown
	{
		public static string RenderMarkdown(this string markdown, MarkdownRenderContext context)
		{
			var texReplacer = new TexReplacer(markdown);

			var markdownObject = new ExtendedMarkdownDeep(context)
			{
				NewWindowForExternalLinks = true,
				ExtraMode = true,
				SafeMode = false,
				MarkdownInHtml = false,
			};

			markdownObject.FormatCodeBlock += FormatCodePrettyPrint;
			var html = markdownObject.Transform(texReplacer.ReplacedText);
			return texReplacer.PlaceTexInsertsBack(html);
		}

		public static HtmlString RenderTex(this string textWithTex)
		{
			var texReplacer = new TexReplacer(textWithTex);
			string html = WebUtility.HtmlEncode(texReplacer.ReplacedText);
			return new HtmlString(texReplacer.PlaceTexInsertsBack(html));
		}

		public static readonly Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);

		public static string FormatCodePrettyPrint(MarkdownDeep.Markdown m, string code)
		{
			code = code.TrimEnd();
			
			// Try to extract the language from the first line
			var match = rxExtractLanguage.Match(code);
			string language = null;

			if (match.Success)
			{
				// Save the language
				var g = match.Groups[2];
				language = g.ToString();

				// Remove the first line
				code = code.Substring(match.Groups[1].Length);
			}

			// If not specified, look for a link definition called "default_syntax" and
			// grab the language from its title
			if (language == null)
			{
				var d = m.GetLinkDefinition("default_syntax");
				if (d != null)
					language = d.title;
			}

			// Common replacements
			if (language == "C#")
				language = "csharp";
			if (language == "C++")
				language = "cpp";

			// Wrap code in pre/code tags and add PrettyPrint attributes if necessary
			if (string.IsNullOrEmpty(language))
				return $"<pre><code>{code}</code></pre>\n";
			return $"<textarea class='code code-sample' data-lang='{language.ToLowerInvariant()}'>{code}</textarea>\n";
		}

		public static (string Html, List<string> RelativeToCourseUrls) GetHtmlWithUrls(this string md, string baseUrl = null)
		{
			var texReplacer = new EdxTexReplacer(md);

			var markdownObject = new Markdown2(baseUrl, false)
			{
				NewWindowForExternalLinks = true,
				ExtraMode = true,
				SafeMode = false,
				MarkdownInHtml = false,
			};

			var relativeUrls = new List<string>();
			markdownObject.RelativeUrl += relativeUrls.Add;

			//markdownObject.FormatCodeBlock += FormatCodePrettyPrint;
			var html = markdownObject.Transform(texReplacer.ReplacedText);
			html = texReplacer.PlaceTexInsertsBack(html);
			return (html, relativeUrls);
		}

		private class ExtendedMarkdownDeep : MarkdownDeep.Markdown
		{
			private MarkdownRenderContext context;

			public ExtendedMarkdownDeep(MarkdownRenderContext context)
			{
				this.context = context;
			}

			private readonly Regex fileToDownloadLinkRegex = new Regex(@".*\.(zip|odp|pptx|docx|xlsx|pdf|mmap|xmind)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			public override void OnPrepareLink(HtmlTag tag)
			{
				base.OnPrepareLink(tag);
				var href = tag.attributes["href"];

				var isFileToDownload = fileToDownloadLinkRegex.IsMatch(href);
				if (isFileToDownload)
					tag.attributes["download"] = "";

				if (!IsAbsoluteUrl(href)) {
					if (!href.StartsWith("/") && IsFile(href))
						tag.attributes["href"] = CourseUrlHelper.GetAbsoluteUrlToFile(context.BaseUrlApi, context.CourseId, context.UnitDirectoryRelativeToCourse, href);
					else
						tag.attributes["href"] = CourseUrlHelper.GetAbsoluteUrl(context.BaseUrlWeb, href);
				}
				else
				{
					if (IsUlearnUrl(href))
						tag.attributes.Remove("target");
				}
			}

			public override void OnPrepareImage(HtmlTag tag, bool TitledImage)
			{
				base.OnPrepareImage(tag, TitledImage);
				tag.attributes["class"] = "slide-image";

				var src = tag.attributes["src"];
				if (!IsAbsoluteUrl(src)) 
					tag.attributes["src"] = CourseUrlHelper.GetAbsoluteUrlToFile(context.BaseUrlApi, context.CourseId, context.UnitDirectoryRelativeToCourse, src);
			}

			private bool IsAbsoluteUrl(string url)
			{
				return Uri.TryCreate(url, UriKind.Absolute, out _);
			}

			private bool IsUlearnUrl(string url)
			{
				return url.IndexOf(context.BaseUrlApi, StringComparison.OrdinalIgnoreCase) >= 0
						|| url.IndexOf(context.BaseUrlWeb, StringComparison.OrdinalIgnoreCase) >= 0;
			}

			private readonly Regex fileLinkRegex = new Regex(@".*\.\w{1,5}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			private bool IsFile(string url)
			{
				return fileLinkRegex.IsMatch(url);
			}
		}
	}
}