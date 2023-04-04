using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common.SuperGroup;


[DataContract]
public abstract class ValidatingResult
{
	[DataMember]
	public abstract ValidationType Type { get; }
}

[DataContract]
public class InvalidSheetStructure : ValidatingResult
{
	[DataMember]
	public override ValidationType Type => ValidationType.InvalidSheetStructure;
}

[DataContract]
public class GroupsHasSameStudents : ValidatingResult
{
	[DataMember]
	public override ValidationType Type => ValidationType.GroupsHasSameStudents;

	[DataMember]
	public Dictionary<string, string[]> SameNamesInGroups { get; set; }
}

[DataContract]
public class StudentBelongsToOtherGroup : ValidatingResult
{
	[DataMember]
	public override ValidationType Type => ValidationType.StudentBelongsToOtherGroup;

	[DataMember]
	public Dictionary<string, MoveStudentInfo> NeededMoves { get; set; }
}