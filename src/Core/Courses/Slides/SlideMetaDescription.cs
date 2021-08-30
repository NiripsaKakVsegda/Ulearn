using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides
{
	public class SlideMetaDescription
	{
		[XmlElement("image")]
		public string Image { get; set; } // Путь относительно файла слайда

		[XmlElement("keywords")]
		public string Keywords { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }
	}
}