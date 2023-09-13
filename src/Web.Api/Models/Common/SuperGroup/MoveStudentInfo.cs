using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Ulearn.Web.Api.Models.Common.SuperGroup;

[DataContract]
public class MoveStudentInfo
{
	[DataMember]
	[CanBeNull]
	public string FromGroupName { get; set; }

	[DataMember]
	public string ToGroupName { get; set; }
}