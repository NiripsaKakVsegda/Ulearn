using System;
using System.Text.RegularExpressions;
using MarkdownDeep;
using Ulearn.Core.Courses;

namespace Ulearn.Core.Markdown
{
	internal class ExtendedMarkdownDeep : MarkdownDeep.Markdown
	{
		protected MarkdownRenderContext context;

		public ExtendedMarkdownDeep(MarkdownRenderContext context)
		{
			this.context = context;
		}

		private readonly Regex fileToDownloadLinkRegex = new Regex(@".*\.(zip|odp|pptx|docx|xlsx|pdf|mmap|xmind)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public override void OnPrepareLink(HtmlTag tag)
		{
			base.OnPrepareLink(tag);
			var href = tag.attributes["href"];
			if (href.StartsWith("./"))
				href = href.Substring(2);

			var isFileToDownload = fileToDownloadLinkRegex.IsMatch(href);
			if (isFileToDownload)
				tag.attributes["download"] = "";

			if (!IsAbsoluteUrl(href))
			{
				if (!href.StartsWith("/") && IsFile(href))
					tag.attributes["href"] = GetLinkToFile(href);
				else
					tag.attributes["href"] = href;
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
				tag.attributes["src"] = GetLinkToFile(src);
		}

		protected virtual string GetLinkToFile(string pathFromUnit)
		{
			return CourseUrlHelper.GetAbsoluteUrlToFile(context.BaseUrlApi, context.CourseId, context.UnitDirectoryRelativeToCourse, pathFromUnit);
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

		private readonly Regex fileLinkRegex = new Regex(@".*\.\w{1,5}(?:$|#|\?|&)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private bool IsFile(string url)
		{
			return fileLinkRegex.IsMatch(url);
		}
	}
}