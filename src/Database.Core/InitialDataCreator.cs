using System;
using System.IO;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Database
{
	public class InitialDataCreator
	{
		private readonly UlearnDb db;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly UlearnUserManager userManager;
		private readonly IUsersRepo usersRepo;
		private readonly IFeedRepo feedRepo;
		private readonly ICoursesRepo coursesRepo;

		private readonly string sysAdminRole = LmsRoleType.SysAdmin.ToString();

		public InitialDataCreator(
			UlearnDb db,
			RoleManager<IdentityRole> roleManager,
			UlearnUserManager userManager,
			IUsersRepo usersRepo,
			IFeedRepo feedRepo,
			ICoursesRepo coursesRepo
		)
		{
			this.db = db;
			this.roleManager = roleManager;
			this.userManager = userManager;
			this.usersRepo = usersRepo;
			this.feedRepo = feedRepo;
			this.coursesRepo = coursesRepo;
		}

		public async Task CreateAllAsync()
		{
			await CreateRoles().ConfigureAwait(false);
			await CreateUsers().ConfigureAwait(false);
			await CreateUlearnBotUser().ConfigureAwait(false);
			await AddFeedNotificationTransport().ConfigureAwait(false);
			await AddExampleCourse().ConfigureAwait(false);
		}

		public async Task CreateRoles()
		{
			if (!await db.Roles.AnyAsync(r => r.Name == sysAdminRole).ConfigureAwait(false))
			{
				await roleManager.CreateAsync(new IdentityRole(sysAdminRole)).ConfigureAwait(false);
			}

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		private async Task CreateUsers()
		{
			if (!await db.Users.AnyAsync(u => u.UserName == "user").ConfigureAwait(false))
			{
				var user = new ApplicationUser { UserName = "user", FirstName = "User", LastName = "" };
				await userManager.CreateAsync(user, "asdasd").ConfigureAwait(false);
			}

			if (!await db.Users.AnyAsync(u => u.UserName == "admin").ConfigureAwait(false))
			{
				var user = new ApplicationUser { UserName = "admin", FirstName = "System Administrator", LastName = "" };
				await userManager.CreateAsync(user, "fullcontrol").ConfigureAwait(false);
				await userManager.AddToRoleAsync(user, sysAdminRole).ConfigureAwait(false);
			}

			await CreateRoles().ConfigureAwait(false);
		}

		public async Task CreateUlearnBotUser()
		{
			await usersRepo.CreateUlearnBotUserIfNotExists().ConfigureAwait(false);

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		private async Task AddFeedNotificationTransport()
		{
			await feedRepo.AddFeedNotificationTransportIfNeeded(null);
		}

		public async Task AddExampleCourse()
		{
			var courseId = CoursesRepo.ExampleCourseId;
			var hasCourse = await coursesRepo.GetPublishedCourseVersion(courseId) != null;
			if (!hasCourse)
			{
				var versionId = Guid.NewGuid();
				var userId = await usersRepo.GetUlearnBotUserId();
				var zipFileContent = await File.ReadAllBytesAsync("Help.zip");
				await coursesRepo.AddCourseVersion(courseId, versionId, userId, null, null, null, null, zipFileContent);
				await coursesRepo.MarkCourseVersionAsPublished(versionId);
			}
		}
	}
}