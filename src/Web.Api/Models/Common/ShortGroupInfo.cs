using System.Runtime.Serialization;
using Database.Models;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class ShortGroupInfo
	{
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public GroupType GroupType { get; set; }

		[DataMember]
		public string CourseId { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsArchived { get; set; }

		[DataMember]
		public ShortUserInfo Owner { get; set; }

		[DataMember]
		public int MembersCount { get; set; }

		[DataMember]
		public string ApiUrl { get; set; }
	}
}