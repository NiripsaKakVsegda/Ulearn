using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Database;
using Database.Models;

namespace ManualUtils
{
	public static class GetIpAddresses
	{
		// Шаблон для выгрузки ip-адресов студентов курсов, не состоящих ни в одной группе, заходивших за n месяцев.
		public static void Run(UlearnDb db, int lastMonthCount, string[] courses, bool isNotMembersOfGroups, bool onlyRegisteredFrom)
		{
			var time = DateTime.Now.AddMonths(-lastMonthCount);
			var membersOfGroups = new HashSet<string>(db.GroupMembers.Select(gm => gm.UserId).Distinct());
			var data = db.Visits
				.Where(v =>
					courses.Contains(v.CourseId)
					&& v.Timestamp > time
					&& v.IpAddress != null
					&& v.User.EmailConfirmed
					&& v.User.Email != null
					&& (!onlyRegisteredFrom || v.User.Registered > time)
				)
				.Select(v => v.UserId)
				.Distinct()
				.Select(id => new {
						User = db.Users.FirstOrDefault(t => t.Id == id),
						IpAddress = db.Visits.Where(t => t.UserId == id && t.IpAddress != null).OrderByDescending(t => t.Timestamp).FirstOrDefault().IpAddress
					}
				)
				.ToList();
			if (isNotMembersOfGroups)
				data = data.Where(v => !membersOfGroups.Contains(v.User.Id))
				.ToList();
			var dataWithVk = new List<(ApplicationUser User, string IpAddress, string VK)>();
			foreach (var t in data)
			{
				string vk = null;
				var vkLogin = db.UserLogins.FirstOrDefault(l => l.LoginProvider == "ВКонтакте" && l.UserId == t.User.Id);
				if (vkLogin != null)
					vk = $"https://vk.com/id{vkLogin.ProviderKey}";
				dataWithVk.Add((t.User, t.IpAddress, vk));
			}

			File.WriteAllText("students.txt", "UserName\tFirstName\tLastName\tEmail\tVK\tIpAddress");
			File.WriteAllLines("students.txt", dataWithVk.Select(v => $"{v.User.UserName}\t{v.User.FirstName}\t{v.User.LastName}\t{v.User.Email}\t{v.VK}\t{v.IpAddress}"));
		}
	}
}