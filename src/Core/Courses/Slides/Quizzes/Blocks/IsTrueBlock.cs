using System;
using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

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

		[Obsolete("Не используется, т.к. тесты показываются как iframe")]
		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			var items = new[]
			{
				new Choice { Correct = Answer, Text = "true" },
				new Choice { Correct = !Answer, Text = "false" }
			};
			var cg = new MultipleChoiceGroup { Label = Text, Type = "MultipleChoice", Choices = items };
			return new MultipleChoiceComponent
			{
				UrlName = context.Slide.NormalizedGuid + context.ComponentIndex,
				ChoiceResponse = new MultipleChoiceResponse { ChoiceGroup = cg },
				Title = EdxTexReplacer.ReplaceTex(Text),
				Solution = new Solution(Explanation)
			};
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