﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common;
using Ulearn.Web.Api.Models.Parameters.Users;
using Ulearn.Web.Api.Models.Responses.Users;

namespace Ulearn.Web.Api.Controllers.Users
{
	[Route("/users")]
	public class UsersController : BaseController
	{
		private readonly IUserSearcher userSearcher;
		private readonly ICourseRolesRepo courseRolesRepo;

		public UsersController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo, IUserSearcher userSearcher, ICourseRolesRepo courseRolesRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.userSearcher = userSearcher;
			this.courseRolesRepo = courseRolesRepo;
		}

		/// <summary>
		/// Поиск пользователей
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<UsersSearchResponse>> Search([FromQuery] UsersSearchParameters parameters)
		{
			var words = parameters.Query?.Split(' ', '\t').ToList() ?? new List<string>();
			if (words.Count > 10)
				return BadRequest(new ErrorResponse("Too many words in query"));

			var currentUser = await usersRepo.FindUserById(UserId).ConfigureAwait(false);
			var isSystemAdministrator = usersRepo.IsSystemAdministrator(currentUser);

			if (!string.IsNullOrEmpty(parameters.CourseId))
			{
				if (!parameters.CourseRoleType.HasValue)
					return BadRequest(new ErrorResponse("You should specify courseRole with courseId"));
				if (parameters.CourseRoleType == CourseRoleType.Student)
					return BadRequest(new ErrorResponse("You can not search students by this method: there are too many students"));

				/* Only instructors can search by course role */
				var isInstructor = await courseRolesRepo.HasUserAccessToCourse(UserId, parameters.CourseId, CourseRoleType.Instructor).ConfigureAwait(false);
				if (!isInstructor)
					return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorResponse("Only instructors can search by course role"));
			}
			else if (parameters.CourseRoleType.HasValue)
			{
				/* Only sys-admins can search all instructors or all course-admins */
				if (!isSystemAdministrator)
					return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorResponse("Only system administrator can search by course role without specified courseId"));
			}

			if (parameters.LmsRoleType.HasValue)
			{
				if (!isSystemAdministrator)
					return StatusCode((int)HttpStatusCode.Unauthorized, new ErrorResponse("Only system administrator can search by lms role"));
			}

			var request = new UserSearchRequest
			{
				CurrentUser = currentUser,
				Words = words,
				CourseId = parameters.CourseId,
				MinCourseRoleType = parameters.CourseRoleType,
				LmsRole = parameters.LmsRoleType,
			};

			/* Start the search!
			 * First of all we will try to find `strict` users: users with strict match for pattern. These users will be at first place in the response.
			 */

			var strictUsers = await userSearcher.SearchUsersAsync(request, strict: true, offset: 0, count: parameters.Offset + parameters.Count).ConfigureAwait(false);

			var users = strictUsers.ToList();

			/* If strict users count is enough for answer, just take needed piece of list */
			if (users.Count >= parameters.Offset + parameters.Count)
			{
				users = users.Skip(parameters.Offset).Take(parameters.Count).ToList();
			}
			else
			{
				/* If there is part of strict users which we should return, then cut off it */
				if (parameters.Offset < users.Count)
					users = users.Skip(parameters.Offset).ToList();
				else
					users.Clear();

				/*
				 *  (strict users) (non-strict users)
				 *  0     1    2    3    4    5    6
				 *             ^              ^
				 *             offset         offset+count
				 */
				var nonStrictUsers = await userSearcher.SearchUsersAsync(request, strict: false, offset: parameters.Offset - strictUsers.Count, count: parameters.Count - users.Count).ConfigureAwait(false);

				/* Add all non-strict users if there is no this user in strict users list */
				foreach (var user in nonStrictUsers)
				{
					var alreadyExistUser = strictUsers.FirstOrDefault(u => u.User.Id == user.User.Id);
					if (alreadyExistUser != null)
						alreadyExistUser.Fields.UnionWith(user.Fields);
					else
						users.Add(user);
				}
			}

			var instructors = await courseRolesRepo.GetListOfUsersWithCourseRole(CourseRoleType.Instructor, null, true).ConfigureAwait(false);
			var currentUserIsInstructor = instructors.Contains(User.GetUserId());
			return new UsersSearchResponse
			{
				Users = users.Select(u => new FoundUserResponse
				{
					User = BuildShortUserInfo(u.User,
						discloseLogin: u.Fields.Contains(SearchField.Login) || currentUserIsInstructor && instructors.Contains(u.User.Id),
						discloseEmail: u.Fields.Contains(SearchField.Email)),
					Fields = u.Fields.ToList(),
				}).ToList(),
				Pagination = new PaginationResponse
				{
					Offset = parameters.Offset,
					Count = users.Count,
				}
			};
		}

		[HttpGet("{userId}")]
		public async Task<ActionResult<ShortUserInfo>> FindUserById([FromRoute] string userId)
		{
			var currentUser = await usersRepo.FindUserById(UserId);

			if (UserId == userId)
				return BuildShortUserInfo(currentUser, true, true);

			var user = await usersRepo.FindUserById(userId);

			if (user == null)
				return StatusCode((int)HttpStatusCode.NotFound, $"No user with id {userId} found");

			var isSystemAdministrator = usersRepo.IsSystemAdministrator(currentUser);

			return isSystemAdministrator
				? BuildShortUserInfo(user, true, true)
				: BuildShortUserInfo(user, true);
		}

		[HttpGet("by-ids")]
		[Authorize]
		public async Task<ActionResult<UsersByIdsResponse>> FindUsersByIds(
			[FromQuery] List<string> userIds,
			[FromQuery] [CanBeNull] string courseId
		)
		{
			const int maxRequestUsersCount = 100;

			if (userIds.Count > maxRequestUsersCount)
				return StatusCode((int)HttpStatusCode.RequestEntityTooLarge, new ErrorResponse($"You cannot request more than {maxRequestUsersCount} users"));

			var isSysAdmin = await IsSystemAdministratorAsync();

			if (courseId is null && !isSysAdmin)
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only system administrator can request users in all courses"));

			if (courseId is not null)
			{
				var isInstructor = isSysAdmin || await courseRolesRepo.HasUserAccessToCourse(UserId, courseId, CourseRoleType.Instructor);
				if (!isInstructor)
					return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You should be at least instructor of course"));
			}

			var users = await usersRepo.FindUsersFilterAvailableForUser(userIds, UserId, courseId);

			return new UsersByIdsResponse
			{
				FoundUsers = users
					.Select(u => BuildShortUserInfo(u))
					.ToList(),
				NotFoundUserIds = userIds
					.Except(users.Select(u => u.Id))
					.ToList()
			};
		}
	}
}