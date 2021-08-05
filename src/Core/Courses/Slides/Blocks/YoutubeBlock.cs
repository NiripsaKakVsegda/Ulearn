using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("youtube")]
	public class YoutubeBlock : SlideBlock
	{
		[XmlText]
		public string VideoId { get; set; }

		public YoutubeBlock(string videoId)
		{
			VideoId = videoId;
		}

		public YoutubeBlock()
		{
		}

		public override string ToString()
		{
			return $"Video {GetYoutubeUrl()}";
		}

		public VideoComponent GetVideoComponent(string displayName, Slide slide, int componentIndex)
		{
			return new VideoComponent(slide.NormalizedGuid + componentIndex, displayName, VideoId);
		}

		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			throw new NotSupportedException();
		}

		public string GetYoutubeUrl()
		{
			return $"https://youtube.com/watch?v={VideoId}";
		}
	}
}