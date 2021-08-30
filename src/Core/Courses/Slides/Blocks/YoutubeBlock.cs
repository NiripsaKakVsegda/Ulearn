using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("youtube")]
	public class YoutubeBlock : SlideBlock, IConvertibleToEdx
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

		public Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			return new VideoComponent(context.Slide.NormalizedGuid + context.ComponentIndex, context.DisplayName, VideoId);
		}

		public string GetYoutubeUrl()
		{
			return $"https://youtube.com/watch?v={VideoId}";
		}
	}
}