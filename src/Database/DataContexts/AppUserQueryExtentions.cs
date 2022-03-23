using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Database.DataContexts
{
	public static class AppUserQueryExtentions
	{
		public static IQueryable<ApplicationUser> FilterByRole(this IQueryable<ApplicationUser> applicationUsers, IdentityRole role)
		{
			return role == null
				? applicationUsers
				: applicationUsers.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
		}

		public static IQueryable<ApplicationUser> FilterByUserIds(this IQueryable<ApplicationUser> applicationUsers, params List<string>[] idLists)
		{
			HashSet<string> goodIds = null;
			foreach (var idList in idLists.Where(idList => idList != null))
			{
				if (goodIds == null)
					goodIds = new HashSet<string>(idList);
				else
					goodIds.IntersectWith(idList);
			}

			return goodIds == null
				? applicationUsers
				: applicationUsers.Where(user => goodIds.Contains(user.Id));
		}

		/* Pass count=0 to disable limiting */
		public static List<UserRolesInfo> GetUserRolesInfo(this IQueryable<ApplicationUser> applicationUsers, int count, List<IdentityRole> roles)
		{
			IQueryable<ApplicationUser> users = applicationUsers.OrderBy(u => u.UserName);
			if (count > 0)
				users = users.Take(count);

			return users.ToList()
				.Select(user => new UserRolesInfo
				{
					UserId = user.Id,
					UserName = user.UserName,
					UserVisibleName = user.VisibleName,
					Roles = user.Roles.Select(r => roles.FirstOrDefault(rr => rr.Id == r.RoleId)?.Name).Where(r => r != null).ToList()
				}).ToList();
		}
	}
}