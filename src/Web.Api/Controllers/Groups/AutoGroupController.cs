using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Controllers.Groups;

[Route("/autogroup")]
public class AutoGroupController : BaseController
{
	private readonly AutoGroupManager autoGroupHelper;

	public AutoGroupController(
		ICourseStorage courseStorage, 
		UlearnDb db, 
		IUsersRepo usersRepo, 
		AutoGroupManager autoGroupHelper)
		: base(courseStorage, db, usersRepo)
	{
		this.autoGroupHelper = autoGroupHelper;
	}

	[HttpGet]
	[Route("extract-from-table")]
	public async Task<ActionResult<AutoGroupMergeResult>> ExtractFromTable([FromQuery] string tableLink, [FromQuery] int superGroupId)
	{
		var remoteData = await autoGroupHelper.GetRemoteDataAsync(tableLink);
		var localData = await autoGroupHelper.GetLocalDataAsync(superGroupId);
		var merge = BuildMergeResult(localData, remoteData);
		
		return merge;
	}

	private AutoGroupMergeResult BuildMergeResult(string[] localGroups, (string group, string student)[] remoteData)
	{
		var remoteGroups = remoteData.Select(x => x.group).ToHashSet();

		var newGroups = remoteGroups.Where(x => !localGroups.Contains(x)).ToHashSet();

		var groupToLengthMap = newGroups.ToDictionary(x => x, x => remoteData.Count(y => y.group == x));
		var doubledStudentToGroupMap = remoteData.GroupBy(x => x.student)
			.Where(x => x.Count() > 1)
			.ToDictionary(x => x.Key,
				x => x.Select(data1 => data1.group).ToArray());

		return new AutoGroupMergeResult
		{
			NewGroups = newGroups.ToArray(),
			NewGroupsLengths = groupToLengthMap,
			Errors = doubledStudentToGroupMap.Count > 0
				? new AutoGroupMergeError[] { new GroupsHasSameStudentsWarning { StudentToGroupsMap = doubledStudentToGroupMap } }
				: Array.Empty<AutoGroupMergeError>()
		};
	}

}