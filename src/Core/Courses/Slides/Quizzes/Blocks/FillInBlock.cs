using System;
using System.Linq;
using System.Xml.Serialization;

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