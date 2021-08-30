using System;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups
{
	[Route("/groups/{inviteHash:guid}")]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	[Authorize]
	public class JoinGroupController : BaseGroupController
	{
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;

		public JoinGroupController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo,
			IGroupsRepo groupsRepo, IGroupMembersRepo groupMembersRepo,
			ISlideCheckingsRepo slideCheckingsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var groupInviteHash = (Guid)context.ActionArguments["inviteHash"];

			var group = await groupsRepo.FindGroupByInviteHashAsync(groupInviteHash).ConfigureAwait(false);
			if (group == null)
			{
				context.Result = NotFound(new ErrorResponse($"Group with invite hash {groupInviteHash} not found"));
				return;
			}

			context.ActionArguments["group"] = group;

			await base.OnActionExecutionAsync(context, next).ConfigureAwait(false);
		}

		/// <summary>
		/// Найти группу по инвайт-хешу.
		/// Группа должна быть не удалена, а инвайт-ссылка в ней — включена.
		/// </summary>
		/// <param name="inviteHash">Инвайт-хеш группы</param>
		[HttpGet]
		public async Task<ActionResult<GroupInfo>> Group(Guid inviteHash)
		{
			var group = await groupsRepo.FindGroupByInviteHashAsync(inviteHash).ConfigureAwait(false);
			var isMember = await groupMembersRepo.IsUserMemberOfGroup(@group.Id, UserId).ConfigureAwait(false);
			return BuildGroupInfo(group, isUserMemberOfGroup: isMember);
		}

		/// <summary>
		/// Вступить в группу по инвайт-хешу.
		/// Группа должна быть не удалена, а инвайт-ссылка в ней — включена.
		/// </summary>
		/// <param name="inviteHash">Инвайт-хеш группы</param>
		[HttpPost("join")]
		[ProducesResponseType((int)HttpStatusCode.Conflict)]
		[SwaggerResponse((int)HttpStatusCode.Conflict, Description = "User is already a student of this group")]
		public async Task<IActionResult> Join(Guid inviteHash)
		{
			var group = await groupsRepo.FindGroupByInviteHashAsync(inviteHash).ConfigureAwait(false);

			var groupMember = await groupMembersRepo.AddUserToGroupAsync(group.Id, UserId).ConfigureAwait(false);
			if (groupMember == null)
				return StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse($"User {UserId} is already a student of group {group.Id}"));

			await slideCheckingsRepo.ResetManualCheckingLimitsForUser(group.CourseId, UserId);

			return Ok(new SuccessResponseWithMessage($"Student {UserId} is added to group {group.Id}"));
		}
	}
}