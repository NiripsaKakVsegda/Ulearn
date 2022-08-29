using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public interface IVisitsRepo : ILastVisitsRepo
	{
		Task<Visit> AddVisit(string courseId, Guid slideId, string userId, string ipAddress);
		Task<Visit> FindVisit(string courseId, Guid slideId, string userId);
		Task<HashSet<Guid>> GetIdOfVisitedSlides(string courseId, string userId);
		Task UpdateScoreForVisit(string courseId, Slide slide, string userId);
		Task RemoveAttempts(string courseId, Guid slideId, string userId);
		Task<Dictionary<Guid, int>> GetScoresForSlides(string courseId, string userId, IEnumerable<Guid> slidesIds = null);
		Task<Dictionary<string, Dictionary<Guid, int>>> GetScoresForSlides(string courseId, IEnumerable<string> userIds, IEnumerable<Guid> slidesIds = null);
		Task<Dictionary<string, HashSet<Guid>>> GetSkippedSlides(string courseId, IEnumerable<string> userIds, IEnumerable<Guid> slidesIds = null);
		Task<List<Guid>> GetSlidesWithUsersManualChecking(string courseId, string userId);
		Task MarkVisitsAsWithManualChecking(string courseId, Guid slideId, string userId);
		Task<int> GetScore(string courseId, Guid slideId, string userId);
		Task<Dictionary<string, int>> GetScore(string courseId, Guid slideId, List<string> userIds);
		Task SkipSlide(string courseId, Guid slideId, string userId);
		Task<bool> IsSkipped(string courseId, Guid slideId, string userId);

		/// Забывает, что пользователь смотрел чужие решения и дает ему получить баллы при следующей отправке.
		Task UnskipAllSlides(string courseId, string userId);

		Task<bool> IsPassed(string courseId, Guid slideId, string userId);
		Task<bool> IsSkippedOrPassed(string courseId, Guid slideId, string userId);
		Task AddVisits(IEnumerable<Visit> visits);
		IQueryable<Visit> GetVisitsInPeriod(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish, IEnumerable<string> usersIds = null);
		IQueryable<Visit> GetVisitsInPeriod(VisitsFilterOptions options);
		Task<Dictionary<Guid, List<Visit>>> GetVisitsInPeriodForEachSlide(VisitsFilterOptions options);
		Task<List<string>> GetUsersVisitedAllSlides(VisitsFilterOptions options);
		Task<HashSet<string>> GetUserCourses(string userId);
		Task<List<string>> GetCourseUsers(string courseId);
		Task<List<RatingEntry>> GetCourseRating(string courseId, int minScore, List<Guid> requiredSlides);
		Task<HashSet<string>> GetUserIdsWithPassedSlide(string courseId, Guid slideId, IEnumerable<string> userIds);
		bool HasManualChecking(string courseId, string userId, Guid slideId);
	}

	public interface ILastVisitsRepo
	{
		Task<LastVisit> FindLastVisit(string courseId, string userId, Guid? slideId = null);
		Task<Dictionary<string, Visit>> FindLastVisitsInGroup(int groupId);
		Task<Dictionary<Guid, LastVisit>> GetLastVisitsInCourse(string courseId, string userId);

		Task<Dictionary<string, DateTime>> GetLastVisitsForCourses(HashSet<string> courseIds, string userId);
	}
}