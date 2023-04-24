namespace Database.Models;

public class GroupSettings
{
	public string NewName { get; set; }
	public bool? NewIsManualCheckingEnabled { get; set; }
	public bool? NewIsManualCheckingEnabledForOldSolutions { get; set; }
	public bool? NewDefaultProhibitFurtherReview { get; set; }
	public bool? NewCanUsersSeeGroupProgress { get; set; }
	public int? SuperGroupId { get; set; }
	public string DistributionTableLink { get; set; }
}