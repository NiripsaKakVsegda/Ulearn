using System;
using System.Runtime.Serialization;
using Database.Models;

namespace Ulearn.Web.Api.Models.Common.SuperGroup;

[DataContract]
public class GroupMemberInfo
{
	[DataMember]
	public int GroupId { get; set; }

	[DataMember]
	public string UserId { get; set; }

	[DataMember]
	public virtual ShortUserInfo User { get; set; }

	[DataMember]
	public DateTime? AddingTime { get; set; }

	public static GroupMemberInfo BuildGroupMemberInfo(GroupMember member, ShortUserInfo user)
	{
		return new GroupMemberInfo
		{
			GroupId = member.GroupId,
			UserId = member.UserId,
			AddingTime = member.AddingTime,
			User = user
		};
	}
}