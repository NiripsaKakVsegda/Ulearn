using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common.SuperGroup;

[DataContract]
public class MoveStudentInfo
{
	[DataMember]

	public string FromGroupName { get; set; }

	[DataMember]
	public string ToGroupName { get; set; }
}