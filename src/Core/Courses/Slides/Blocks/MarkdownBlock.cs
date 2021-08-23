using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using AngleSharp.Html.Parser;
using JetBrains.Annotations;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Markdown;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	// [XmlType("markdown")]
	// [XmlRoot("markdown", Namespace = "https://ulearn.me/schema/v2")]
	public class MarkdownBlock : SlideBlock, IXmlSerializable
	{
		private string markdown;

		[XmlText]
		public string Markdown
		{
			get => markdown;
			set => markdown = value.RemoveCommonNesting();
		}

		[CanBeNull] // null, если состоит их одного элемента. Тогда нужно использовать this 
		public SlideBlock[] InnerBlocks { get; set; } // может содержать MarkdownBlock или CodeBlock

		public MarkdownBlock(string markdown)
		{
			if (markdown != null)
				Markdown = markdown.TrimEnd();
		}

		public MarkdownBlock()
		{
		}

		public string RenderMarkdown(Slide slide, MarkdownRenderContext context)
		{
			return GetMarkdownWithReplacedLinksToStudentZips(Markdown, context.CourseId, slide, context.BaseUrlApi).RenderMarkdown(context);
		}

		/* Replace links to (/Exercise/StudentZip) and to (ExerciseZip): automagically add courseId and slideId */
		private static string GetMarkdownWithReplacedLinksToStudentZips(string markdown, string courseId, Slide slide, string baseUrlApi)
		{
			if (string.IsNullOrEmpty(markdown))
				return "";
			var exerciseSlide = slide as ExerciseSlide;
			if (exerciseSlide == null)
				return markdown;
			if (!(markdown.Contains("(/Exercise/StudentZip)") || markdown.Contains("(ExerciseZip)")))
				return markdown;
			var studentZipName = (exerciseSlide.Exercise as CsProjectExerciseBlock)?.CsprojFileName ?? new DirectoryInfo((exerciseSlide.Exercise as UniversalExerciseBlock).ExerciseDirPath).Name;
			var studentZipFullPath = CourseUrlHelper.GetAbsoluteUrlToStudentZip(baseUrlApi, courseId, slide.Id, $"{studentZipName}.zip");
			return markdown.Replace("(/Exercise/StudentZip)", $"({studentZipFullPath})").Replace("(ExerciseZip)", $"({studentZipFullPath})");
		}

		public override string ToString()
		{
			return $"Markdown {Markdown}";
		}

		public (List<SlideBlock> HtmlAndCodeBlocks, List<StaticFileForEdx> StaticFiles) ToHtmlAndCodeBlocks(
			string ulearnBaseUrlApi, string ulearnBaseUrlWeb, string courseId, Slide slide, DirectoryInfo courseDirectory)
		{
			var htmlAndCodeBlocks = new List<SlideBlock>();
			var allStaticFiles = new List<StaticFileForEdx>();
			var subBlocks = InnerBlocks ?? new[] { this };
			foreach (var subBlock in subBlocks)
			{
				if (subBlock is MarkdownBlock mb)
				{
					var markdownRenderContext = new MarkdownRenderContext(ulearnBaseUrlApi, ulearnBaseUrlWeb, courseId, slide.Unit.UnitDirectoryRelativeToCourse);
					var (html, staticFiles) = GetMarkdownWithReplacedLinksToStudentZips(mb.Markdown, courseId, slide, ulearnBaseUrlApi)
						.RenderMarkdownForEdx(markdownRenderContext, courseDirectory, "/static");
					var parsedBlocks = ParseMarkdownForEdxToHtmlBlocksAndCodeBlocks(html, ulearnBaseUrlApi, subBlock.Hide);
					htmlAndCodeBlocks.AddRange(parsedBlocks);
					allStaticFiles.AddRange(staticFiles);
				}
				else
					htmlAndCodeBlocks.Add(subBlock);
			}

			return (htmlAndCodeBlocks, allStaticFiles);
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			return InnerBlocks?.SelectMany(b => b.BuildUp(context, filesInProgress)) ?? new[] { this };
		}

		public override string TryGetText()
		{
			return Markdown;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			Hide = reader.GetAttribute("hide").IsOneOf("true", "1");
			var blocks = ReadBlocks(Hide, reader).ToArray();
			if (blocks.Length == 1 && blocks[0].GetType() == typeof(MarkdownBlock))
			{
				var mb = (MarkdownBlock)blocks[0];
				Markdown = mb.Markdown;
				Hide = mb.Hide;
			}
			else
				InnerBlocks = blocks;
		}

		private IEnumerable<SlideBlock> ReadBlocks(bool hide, XmlReader reader)
		{
			var tagName = reader.Name;
			if (reader.IsEmptyElement)
			{
				reader.Read();
				yield break;
			}

			reader.Read();
			while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == tagName))
			{
				if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
				{
					yield return new MarkdownBlock(reader.ReadContentAsString()) { Hide = hide };
				}
				else if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "note")
					{
						yield return new MarkdownBlock
						{
							Hide = true,
							Markdown = reader.ReadElementContentAsString()
						};
					}
					else if (reader.LocalName == "code")
					{
						var languageAttribute = reader.GetAttribute("language");
						Language? language = null;
						if (!string.IsNullOrEmpty(languageAttribute))
							language = LanguageHelpers.ParseFromXml(languageAttribute);
						yield return new CodeBlock(reader.ReadElementContentAsString(), language) { Hide = hide };
					}
					else
						throw new NotSupportedException(
							$"Invalid tag inside of <markdown>: {reader.LocalName}. Supported tags inside <markdown> are <note> and <code>."
						);
				}
				else
					reader.Read();
			}

			reader.Read();
		}

		public void WriteXml(XmlWriter writer)
		{
			if (Hide)
				writer.WriteAttributeString("hide", "true");
			writer.WriteString(markdown);
		}

		private static List<SlideBlock> ParseMarkdownForEdxToHtmlBlocksAndCodeBlocks(string renderedMarkdown, string ulearnBaseUrlApi, bool hide)
		{
			var parser = new HtmlParser();
			var document = parser.ParseDocument(renderedMarkdown);
			var rootElements = document.Body.Children;
			var blocks = new List<SlideBlock>();
			foreach (var element in rootElements)
			{
				var tagName = element.TagName.ToLower();
				if (tagName == "textarea")
				{
					var langStr = element.GetAttribute("data-lang");
					var lang = (Language)Enum.Parse(typeof(Language), langStr, true);
					var code = element.TextContent;
					blocks.Add(new CodeBlock(code, lang) { Hide = hide });
				}
				else
				{
					var htmlContent = element.OuterHtml;
					if (blocks.Count > 0 && blocks.Last() is HtmlBlock last)
					{
						htmlContent = last.GetContent(ulearnBaseUrlApi) + "\n" + htmlContent;
						blocks[blocks.Count - 1] = new HtmlBlock(htmlContent) { Hide = hide };
					}
					else
						blocks.Add(new HtmlBlock(htmlContent) { Hide = hide });
				}
			}

			return blocks;
		}
	}
}