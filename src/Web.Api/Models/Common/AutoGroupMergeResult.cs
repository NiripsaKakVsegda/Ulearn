using System.Collections.Generic;

namespace Ulearn.Web.Api.Models.Common;

public class AutoGroupMergeResult
{
	public AutoGroupMergeError[] Errors { get; set; }
	public string[] NewGroups { get; set; }
	public Dictionary<string, int> NewGroupsLengths { get; set; }
}

public enum AutoGroupTableParsingErrorType
{
	ParsingError,
	GroupsHasSameStudents
}

public abstract class AutoGroupMergeError
{
	public abstract AutoGroupTableParsingErrorType ErrorType { get; }
}

public class GeneralParsingError : AutoGroupMergeError
{
	public override AutoGroupTableParsingErrorType ErrorType => AutoGroupTableParsingErrorType.ParsingError;
}

public class GroupsHasSameStudentsWarning : AutoGroupMergeError
{
	public override AutoGroupTableParsingErrorType ErrorType => AutoGroupTableParsingErrorType.GroupsHasSameStudents;
	
	public Dictionary<string, string[]> StudentToGroupsMap { get; set; }
}