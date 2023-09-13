using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common;
using Ulearn.Core;

namespace Database.Repos.Users
{
	public class UsersRepo : IUsersRepo
	{
		private readonly UlearnDb db;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly Lazy<ICourseRolesRepo> courseRolesRepo;
		private IdentityRole sysAdminRole;

		public const string UlearnBotUsername = "ulearn-bot";

		public UsersRepo(UlearnDb db, UlearnUserManager userManager, IServiceProvider serviceProvider)
		{
			this.db = db;
			this.userManager = userManager;
			courseRolesRepo = new Lazy<ICourseRolesRepo>(serviceProvider.GetRequiredService<ICourseRolesRepo>);
		}

		public async Task<ApplicationUser> FindUserById(string userId)
		{
			var user = await db.Users.FindAsync(userId);
			return user == null || user.IsDeleted ? null : user;
		}

		public Task<bool> IsUserExist(string userId)
		{
			return db.Users.AnyAsync(user => user.Id == userId && !user.IsDeleted);
		}

		public Task<List<string>> FindUsersBySocialProviderKey(string providerKey)
		{
			return db.UserLogins.Where(login => login.ProviderKey == providerKey).Select(login => login.UserId).Distinct().ToListAsync();
		}

		public async Task<List<string>> GetUserIdsWithLmsRole(LmsRoleType lmsRole)
		{
			var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == lmsRole.ToString());
			if (role == null)
				return new List<string>();
			return await db.Users.Where(u => !u.IsDeleted).FilterByRole(role).Select(u => u.Id).ToListAsync();
		}

		public async Task<List<string>> GetSysAdminsIds()
		{
			return await GetUserIdsWithLmsRole(LmsRoleType.SysAdmin);
		}

		public async Task ChangeTelegram(string userId, long? chatId, string chatTitle)
		{
			var user = await FindUserById(userId);
			if (user == null)
				return;

			user.TelegramChatId = chatId;
			user.TelegramChatTitle = chatTitle;
			await db.SaveChangesAsync();
		}

		public async Task ConfirmEmail(string userId, bool isConfirmed = true)
		{
			var user = await FindUserById(userId);
			if (user == null)
				return;

			user.EmailConfirmed = isConfirmed;
			await db.SaveChangesAsync();
		}

		public Task UpdateLastConfirmationEmailTime(ApplicationUser user)
		{
			user.LastConfirmationEmailTime = DateTime.Now;
			return db.SaveChangesAsync();
		}

		public Task ChangeEmail(ApplicationUser user, string email)
		{
			user.Email = email;
			user.EmailConfirmed = false;
			return db.SaveChangesAsync();
		}

		[NotNull]
		public async Task<ApplicationUser> GetUlearnBotUser()
		{
			var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == UlearnBotUsername);
			if (user == null)
				throw new NotFoundException($"Ulearn bot user (username = {UlearnBotUsername}) not found");
			return user;
		}

		public async Task<string> GetUlearnBotUserId()
		{
			var user = await GetUlearnBotUser();
			return user.Id;
		}

		public async Task CreateUlearnBotUserIfNotExists()
		{
			var ulearnBotFound = await db.Users.AnyAsync(u => u.UserName == UlearnBotUsername);
			if (!ulearnBotFound)
			{
				var user = new ApplicationUser
				{
					UserName = UlearnBotUsername,
					FirstName = "Ulearn",
					LastName = "bot",
					Email = "support@ulearn.me",
				};
				await userManager.CreateAsync(user, StringUtils.GenerateSecureAlphanumericString(10));

				await db.SaveChangesAsync();
			}
		}

		public async Task<List<ApplicationUser>> FindUsersByUsernameOrEmail(string usernameOrEmail)
		{
			return await db.Users.Where(u => (u.UserName == usernameOrEmail || u.Email == usernameOrEmail) && !u.IsDeleted).ToListAsync();
		}

		public Task<List<ApplicationUser>> GetUsersByIds(IEnumerable<string> usersIds)
		{
			return db.Users.Where(u => usersIds.Contains(u.Id) && !u.IsDeleted).ToListAsync();
		}

		public Task DeleteUserAsync(ApplicationUser user)
		{
			user.IsDeleted = true;
			/* Change name to make creating new user with same name possible */
			user.UserName = user.UserName + "__deleted__" + (new Random().Next(100000));
			return db.SaveChangesAsync();
		}

		public bool IsSystemAdministrator(ApplicationUser user)
		{
			if (user?.Roles == null)
				return false;

			if (sysAdminRole == null)
				sysAdminRole = db.Roles.First(r => r.Name == LmsRoleType.SysAdmin.ToString());

			return user.Roles.Any(role => role.RoleId == sysAdminRole.Id);
		}

		public async Task<bool> IsSystemAdministrator(string userId)
		{
			return (await GetSysAdminsIds()).Contains(userId);
		}

		public async Task<List<ApplicationUser>> FindUsersByConfirmedEmails(IEnumerable<string> emails)
		{
			return await db.Users.Where(u => emails.Contains(u.Email) && u.EmailConfirmed).ToListAsync();
		}

		public async Task<List<ApplicationUser>> FindUsersByConfirmedEmail(string email)
		{
			return await FindUsersByConfirmedEmails(new[] { email });
		}

		public async Task<List<ApplicationUser>> FindUsersFilterAvailableForUser(IEnumerable<string> userIds, string userId, string courseId = null)
		{
			var query = await FilterUsersAvailableForUser_Internal(userIds, userId, courseId);
			return query is null
				? new List<ApplicationUser>()
				: await query
					.ToListAsync();
		}

		public async Task<List<string>> FilterUserIdsAvailableForUser(IEnumerable<string> userIds, string userId, string courseId = null)
		{
			var query = await FilterUsersAvailableForUser_Internal(userIds, userId, courseId);
			return query is null
				? new List<string>()
				: await query
					.Select(u => u.Id)
					.ToListAsync();
		}

		[ItemCanBeNull]
		private async Task<IQueryable<ApplicationUser>> FilterUsersAvailableForUser_Internal(IEnumerable<string> userIds, string userId, string courseId = null)
		{
			userIds = userIds
				.Distinct()
				.ToArray();

			var query = db.Users
				.Where(u => userIds.Contains(u.Id) && !u.IsDeleted);

			var isCourseAdmin = await courseRolesRepo.Value.HasUserAccessTo_Any_Course(userId, CourseRoleType.CourseAdmin);
			if (isCourseAdmin)
				return query;

			var userGroupQuery = query
				.GroupJoin(
					db.GroupMembers,
					u => u.Id,
					member => member.UserId,
					(user, members) => new { user, members }
				)
				.SelectMany(
					e => e.members,
					(e, member) => new { e.user, member.Group }
				)
				.GroupJoin(
					db.GroupAccesses,
					e => e.Group.Id,
					ga => ga.GroupId,
					(e, accesses) => new { e.user, e.Group, accesses }
				)
				.SelectMany(
					e => e.accesses.DefaultIfEmpty(),
					(e, access) => new { e.user, e.Group, access }
				);

			if (courseId is not null)
				return await courseRolesRepo.Value.HasUserAccessToCourse(userId, courseId, CourseRoleType.Instructor)
					? userGroupQuery
						.Where(e => e.Group.CourseId == courseId)
						.Where(e => e.Group.OwnerId == userId || (e.access.UserId == userId && e.access.IsEnabled))
						.Select(e => e.user)
						.Distinct()
					: null;

			var instructorCourseIds = await courseRolesRepo.Value.GetCoursesWhereUserIsInRole(userId, CourseRoleType.Instructor);

			return userGroupQuery
				.Where(e =>
					instructorCourseIds.Contains(e.Group.CourseId) &&
					(e.Group.OwnerId == userId || (e.access.UserId == userId && e.access.IsEnabled))
				)
				.Select(e => e.user)
				.Distinct();
		}
	}
}