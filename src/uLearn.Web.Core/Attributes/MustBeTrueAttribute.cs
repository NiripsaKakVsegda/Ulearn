using System.ComponentModel.DataAnnotations;

namespace uLearn.Web.Core.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class MustBeTrueAttribute : ValidationAttribute
{
	public override bool IsValid(object? value) => value is true;
}