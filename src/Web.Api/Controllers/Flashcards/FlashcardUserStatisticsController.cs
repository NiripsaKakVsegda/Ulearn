using Database;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Database.Repos.Flashcards;
using Database.Repos.Groups;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Flashcards;

namespace Ulearn.Web.Api.Controllers.Flashcards
{
	[Route("/flashcard-user-statistics")]
	public class FlashcardUserStatisticsController : BaseController
	{
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IUsersFlashcardsVisitsRepo usersFlashcardsVisitsRepo;
		private readonly IUnitsRepo unitsRepo;

		public FlashcardUserStatisticsController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo, IGroupAccessesRepo groupAccessesRepo, IUsersFlashcardsVisitsRepo usersFlashcardsVisitsRepo,
			IUnitsRepo unitsRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.groupAccessesRepo = groupAccessesRepo;
			this.usersFlashcardsVisitsRepo = usersFlashcardsVisitsRepo;
			this.unitsRepo = unitsRepo;
		}

		[HttpGet]
		public async Task<ActionResult<UserFlashcardStatisticResponse>> UserFlashcardStatistics([FromQuery][BindRequired] string courseId)
		{
			var course = courseStorage.FindCourse(courseId);
			if (course == null)
				return NotFound();
			var groups = (await groupAccessesRepo.GetAvailableForUserGroupsAsync(course.Id, UserId, true, actual: true, archived: false, groupType: GroupQueryType.SingleGroup)).AsGroups().ToArray();
			if (groups.Length == 0)
			{
				return BadRequest("You don't have access to any group in course");
			}

			var result = new UserFlashcardStatisticResponse();
			foreach (var group in groups)
			{
				foreach (var member in group.Members)
				{
					var userStat = await GetUserFlashcardStatistics(member, group, course);
					result.UsersFlashcardsStatistics.Add(userStat);
				}
			}

			return result;
		}

		private async Task<UserFlashcardStatistics> GetUserFlashcardStatistics(GroupMember member, SingleGroup group, Course course)
		{
			var userStat = new UserFlashcardStatistics
			{
				UserId = member.UserId,
				UserName = member.User.VisibleNameWithLastNameFirst,
				GroupId = group.Id,
				GroupName = group.Name
			};
			var userVisits = await usersFlashcardsVisitsRepo.GetLastUserFlashcardsVisitsAsync(member.UserId, course.Id);

			var visitsByUnits = userVisits.GroupBy(x => x.UnitId).ToDictionary(x => x.Key);

			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			foreach (var unit in course.GetUnits(visibleUnitsIds))
			{
				var unitStat = new UnitUserStatistic { UnitId = unit.Id, UnitTitle = unit.Title, TotalFlashcardsCount = unit.Flashcards.Count };
				if (visitsByUnits.TryGetValue(unit.Id, out var unitGroup))
				{
					var rate5FlashcardsIds = new HashSet<string>();
					var uniqueFlashcardsIds = new HashSet<string>();

					foreach (var visit in unitGroup)
					{
						if (visit.Rate == Rate.Rate5)
						{
							rate5FlashcardsIds.Add(visit.FlashcardId);
						}

						uniqueFlashcardsIds.Add(visit.FlashcardId);
					}

					unitStat.Rate5Count = rate5FlashcardsIds.Count;
					unitStat.UniqueFlashcardVisits = uniqueFlashcardsIds.Count;
					unitStat.TotalFlashcardVisits = unitGroup.Count();
				}

				userStat.UnitUserStatistics.Add(unitStat);
			}

			return userStat;
		}
	}
}