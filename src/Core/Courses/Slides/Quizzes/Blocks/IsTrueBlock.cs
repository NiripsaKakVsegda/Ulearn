using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.isTrue")]
	public class IsTrueBlock : AbstractQuestionBlock
	{
		[XmlAttribute("answer")]
		public bool Answer;

		[XmlAttribute("explanation")]
		public string Explanation;

		public bool IsRight(string text)
		{
			return text.ToLower() == Answer.ToString().ToLower();
		}

		public override void Validate(SlideBuildingContext slideBuildingContext)
		{
		}

		public override string TryGetText()
		{
			return Text + '\n' + Explanation;
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			var block = other as IsTrueBlock;
			if (block == null)
				return false;
			return Answer == block.Answer;
		}
	}
}