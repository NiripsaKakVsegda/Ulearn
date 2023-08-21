using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Groups;

[DataContract]
public class GroupsByIdsResponse
{
	[DataMember]
	public List<ShortGroupInfo> FoundGroups { get; set; }

	[DataMember]
	public List<int> NotFoundGroupIds { get; set; }
}