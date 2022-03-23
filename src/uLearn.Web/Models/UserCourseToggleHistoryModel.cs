using System;
using System.Collections.Generic;
using System.Linq;
using Database.Models;
using Ulearn.Core.Courses;

namespace uLearn.Web.Models
{
	public class UserCourseToggleHistoryModel
	{
		public ApplicationUser User { get; set; }
		public Course Course;
		public List<UserToggleModel> UserGrantsHistory;


		public UserCourseToggleHistoryModel(ApplicationUser user, Course course, List<UserToggleModel> rolesHistory, List<UserToggleModel> accessesHistory)
		{
			User = user;
			Course = course;
			UserGrantsHistory = MergeUserRolesAndAccessesGrants(rolesHistory, accessesHistory);
		}

		private List<UserToggleModel> MergeUserRolesAndAccessesGrants(List<UserToggleModel> rolesHistory, List<UserToggleModel> accessesHistory)
		{
			return rolesHistory
				.Concat(accessesHistory)
				.OrderByDescending(x => x.GrantTimeUtc)
				.ToList();
		}
	}


	public class UserToggleModel
	{
		public string GrantedBy { get; set; }
		public string Grant { get; set; }
		public GrantType GrantType { get; set; }
		public DateTime GrantTimeUtc { get; set; }
		public bool IsEnabled { get; set; }
		public string Comment { get; set; }
	}

	public enum GrantType
	{
		Role,
		Access
	}
}