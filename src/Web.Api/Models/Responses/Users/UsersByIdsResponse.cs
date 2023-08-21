using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Users;

[DataContract]
public class UsersByIdsResponse
{
	[DataMember]
	public List<ShortUserInfo> FoundUsers { get; set; }

	[DataMember]
	public List<string> NotFoundUserIds { get; set; }
}