using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using Database.Models;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc.Routing;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups
{
	public abstract class BaseGroupController : BaseController
	{
		protected BaseGroupController(ICourseStorage courseStorage, UlearnDb db, IUsersRepo usersRepo)
			: base(courseStorage, db, usersRepo)
		{
		}

		protected GroupInfo BuildGroupInfo(GroupBase group,
			int? membersCount = null,
			IEnumerable<GroupAccess> accesses = null,
			bool addGroupApiUrl = false,
			bool? isUserMemberOfGroup = null)
		{
			if (group == null)
				throw new ArgumentNullException(nameof(group));

			var isManualCheckingEnabled = false;
			var isManualCheckingEnabledForOldSolutions = false;
			var defaultProhibitFutherReview = false;
			var canUsersSeeGroupProgress = false;

			if (group is SingleGroup singleGroup)
			{
				isManualCheckingEnabled = singleGroup.IsManualCheckingEnabled;
				isManualCheckingEnabledForOldSolutions = singleGroup.IsManualCheckingEnabledForOldSolutions;
				defaultProhibitFutherReview = singleGroup.DefaultProhibitFutherReview;
				canUsersSeeGroupProgress = singleGroup.CanUsersSeeGroupProgress;
			}
			
			return new GroupInfo
			{
				Id = group.Id,
				GroupType = group.GroupType,
				CreateTime = group.CreateTime,
				Name = group.Name,
				Owner = BuildShortUserInfo(group.Owner),
				InviteHash = group.InviteHash,
				IsInviteLinkEnabled = group.IsInviteLinkEnabled,
				IsArchived = group.IsArchived,
				AreYouStudent = isUserMemberOfGroup,

				IsManualCheckingEnabled = isManualCheckingEnabled,
				IsManualCheckingEnabledForOldSolutions = isManualCheckingEnabledForOldSolutions,
				DefaultProhibitFurtherReview = defaultProhibitFutherReview,
				CanStudentsSeeGroupProgress = canUsersSeeGroupProgress,

				StudentsCount = membersCount,
				Accesses = accesses?.Select(BuildGroupAccessesInfo).ToList(),

				ApiUrl = addGroupApiUrl ? Url.Action(new UrlActionContext { Action = nameof(GroupController.Group), Controller = "Group", Values = new { groupId = group.Id } }) : null,
			};
		}

		protected GroupAccessesInfo BuildGroupAccessesInfo(GroupAccess access)
		{
			if (access == null)
				throw new ArgumentNullException(nameof(access));

			return new GroupAccessesInfo
			{
				User = BuildShortUserInfo(access.User),
				AccessType = access.AccessType,
				GrantedBy = BuildShortUserInfo(access.GrantedBy),
				GrantTime = access.GrantTime
			};
		}
	}
}