using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("includeMarkdown")]
	public class IncludeMarkdownBlock : SlideBlock
	{
		[XmlAttribute("file")]
		public string File { get; set; }

		public IncludeMarkdownBlock(string file)
		{
			File = file;
		}

		public IncludeMarkdownBlock()
		{
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			yield return new MarkdownBlock(context.UnitDirectory.GetContent(File)) { Hide = Hide };
		}
	}
}