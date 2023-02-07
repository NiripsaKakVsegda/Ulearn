using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Parameters.Groups;

namespace Ulearn.Web.Api.Models.Common;

[DataContract]
public class ApplyMergeRequest
{
	[DataMember]
	public AutoGroupMergeResult Merge { get; set; }
	
	[DataMember]
	public string DistributionLink { get; set; }
	
	[DataMember]
	public bool IsManualCheckingEnabled { get; set; }
	[DataMember]
	public bool CanStudentsSeeGroupProgress { get; set; }
	[DataMember]
	public bool IsManualCheckingEnabledForOldSolutions { get; set; }
	[DataMember]
	public bool DefaultProhibitFurtherReview { get; set; }
}