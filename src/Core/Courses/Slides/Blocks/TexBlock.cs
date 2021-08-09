using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Markdown;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("tex")]
	public class TexBlock : SlideBlock
	{
		[XmlElement("line")]
		public string[] TexLines { get; set; }

		public TexBlock(string[] texLines)
		{
			TexLines = texLines;
		}

		public TexBlock()
		{
		}

		public override string ToString()
		{
			return $"Tex {string.Join("\n", TexLines)}";
		}

		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			var markdownRenderContext = new MarkdownRenderContext(context.UlearnBaseUrlApi, context.UlearnBaseUrlWeb, context.CourseId, context.Slide.Unit.UnitDirectoryRelativeToCourse);
			var urlName = context.Slide.NormalizedGuid + context.ComponentIndex;
			return new HtmlComponent(
				urlName,
				context.DisplayName,
				urlName,
				// Статические файлы из поля StaticFilesForEdx не берутся. Предполагается, что в tex нет ссылок на файлы.
				string.Join("\n", TexLines.Select(x => "$$" + x + "$$")).RenderMarkdownForEdx(markdownRenderContext, context.CourseDirectory, "/static").Html
			);
		}

		public override string TryGetText()
		{
			return string.Join("\n", TexLines);
		}
	}
}