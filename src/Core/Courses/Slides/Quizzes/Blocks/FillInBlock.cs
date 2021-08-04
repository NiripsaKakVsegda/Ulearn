using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.text")]
	public class FillInBlock : AbstractQuestionBlock
	{
		[XmlElement("sample")]
		public string Sample;

		[XmlElement("regex")]
		public RegexInfo[] Regexes;

		[XmlAttribute("explanation")]
		public string Explanation;

		[XmlAttribute("multiline")]
		public bool Multiline;

		public override void Validate(SlideBuildingContext slideBuildingContext)
		{
			if (string.IsNullOrEmpty(Sample))
				return;
			if (Regexes == null)
				return;
			if (!Regexes.Any(re => re.Regex.IsMatch(Sample)))
				throw new FormatException("Sample should match at least one regex. BlockId=" + Id);
		}

		[Obsolete("Не используется, т.к. тесты показываются как iframe")]
		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			return new TextInputComponent
			{
				UrlName = context.Slide.NormalizedGuid + context.ComponentIndex,
				Title = EdxTexReplacer.ReplaceTex(Text),
				StringResponse = new StringResponse
				{
					Type = (Regexes[0].IgnoreCase ? "ci " : "") + "regexp",
					Answer = "^" + Regexes[0].Pattern + "$",
					AdditionalAnswers = Regexes.Skip(1).Select(x => new Answer { Text = "^" + x.Pattern + "$" }).ToArray(),
					Textline = new Textline { Label = Text, Size = 20 }
				}
			};
		}

		public override string TryGetText()
		{
			return Text + '\n' + Sample + '\t' + Explanation;
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			return other is FillInBlock;
		}
	}
}