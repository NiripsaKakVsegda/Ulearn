using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common.SuperGroup;

namespace Ulearn.Web.Api.Models.Parameters.Groups.SuperGroup;

[DataContract]
public class UpdateGroupsRequestParameters
{
	[DataMember]
	public Dictionary<string, SuperGroupItemActions> GroupsToUpdate { get; set; }
}