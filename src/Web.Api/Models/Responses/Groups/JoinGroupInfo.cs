using System.Runtime.Serialization;
using Database.Models;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Groups;

[DataContract]
public class JoinGroupInfo
{
	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public GroupType GroupType { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string CourseId { get; set; }

	[DataMember]
	public string CourseTitle { get; set; } // To display title for hidden from user course

	[DataMember]
	public ShortUserInfo Owner { get; set; }

	[DataMember]
	public bool IsInviteLinkEnabled { get; set; }

	[DataMember]
	public bool CanStudentsSeeProgress { get; set; }

	[DataMember]
	public bool IsMember { get; set; }

	[DataMember]
	public bool IsInDefaultGroup { get; set; }
}