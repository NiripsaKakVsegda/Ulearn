using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides
{
	public abstract class SlideBlock
	{
		[XmlAttribute("hide")]
		[DefaultValue(false)]
		public bool Hide { get; set; }

		public virtual void Validate(SlideBuildingContext slideBuildingContext)
		{
		}

		public virtual IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			yield return this;
		}

		public virtual string TryGetText()
		{
			return null;
		}
	}
}