using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.order")]
	public class OrderingBlock : AbstractQuestionBlock
	{
		[XmlElement("item")]
		public OrderingItem[] Items;

		[XmlAttribute("explanation")]
		public string Explanation;

		[Obsolete("Не используется, т.к. тесты показываются как iframe")]
		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			throw new NotSupportedException();
		}

		public OrderingItem[] ShuffledItems()
		{
			return Items.Shuffle().ToArray();
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			var block = other as OrderingBlock;
			if (block == null)
				return false;
			return Items.SequenceEqual(block.Items);
		}
	}
}