using System.ComponentModel;
using System.Runtime.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Web.Api.Models.Responses.SlideBlocks
{
	[DataContract]
	[DisplayName("tex")]
	public class TexBlockResponse : IApiSlideBlock
	{
		[DefaultValue(false)]
		[DataMember(Name = "hide", EmitDefaultValue = false)]
		public bool Hide { get; set; }

		[DataMember(Name = "lines")]
		public string[] TexLines { get; set; }

		public TexBlockResponse(TexBlock texBlock)
		{
			Hide = texBlock.Hide;
			TexLines = texBlock.TexLines;
		}
	}
}