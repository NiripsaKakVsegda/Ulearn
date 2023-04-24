using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Database.Models;

public class SuperGroup : GroupBase
{
	[Required]
	public override GroupType GroupType => GroupType.SuperGroup;

	[CanBeNull]
	public string DistributionTableLink { get; set; }
}