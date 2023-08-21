using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Groups;

[DataContract]
public class GroupsSearchResponse
{
	[DataMember]
	public List<ShortGroupInfo> Groups { get; set; }
}