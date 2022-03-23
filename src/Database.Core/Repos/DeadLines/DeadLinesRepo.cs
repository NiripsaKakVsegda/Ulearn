using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public class DeadLinesRepo : IDeadLinesRepo
	{
		private readonly UlearnDb db;

		public DeadLinesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<DeadLine> GetDeadLineById(Guid deadLineId)
		{
			return await db.DeadLines.FindAsync(deadLineId);
		}

		public async Task<List<DeadLine>> GetDeadLines(
			string courseId,
			HashSet<int> groupIds,
			Slide slide = null,
			Guid? userId = null)
		{
			var query = db.DeadLines
				.Where(d => d.CourseId == courseId && groupIds.Contains(d.GroupId));

			if (userId != null)
			{
				query = query
					.Where(d => d.UserIds == null || d.UserIds.Contains(userId.Value));
			}

			if (slide != null)
			{
				query = query
					.Where(d => d.UnitId == slide.Unit.Id);

				if (slide.ScoringGroup != string.Empty)
					query = query
						.Where(d => d.SlideType == DeadLineSlideType.All ||
									d.SlideType == DeadLineSlideType.SlideId && d.SlideValue == slide.Id.ToString() ||
									d.SlideType == DeadLineSlideType.ScoringGroupId && d.SlideValue == slide.ScoringGroup);
				else
					query = query
						.Where(d => d.SlideType == DeadLineSlideType.All ||
									d.SlideType == DeadLineSlideType.SlideId && d.SlideValue == slide.Id.ToString());
			}

			return await query.ToListAsync();
		}

		public async Task<List<DeadLine>> GetDeadLines(
			string courseId,
			int groupId,
			Slide slide = null,
			Guid? userId = null
		)
		{
			return await GetDeadLines(courseId, new HashSet<int> { groupId }, slide, userId);
		}

		public async Task<List<DeadLine>> GetDeadLinesForUser(
			string courseId,
			Guid userId,
			Slide slide = null
		)
		{
			var groups = db.Groups
				.Where(g => g.CourseId == courseId && !g.IsDeleted && !g.IsArchived)
				.Select(g => g.Id);
			var userIdAsString = userId.ToString();
			var userGroupsIds = await db.GroupMembers
				.Where(m => groups.Contains(m.GroupId) && m.UserId == userIdAsString)
				.Select(m => m.GroupId)
				.ToListAsync();

			return await GetDeadLines(courseId, userGroupsIds.ToHashSet(), slide, userId);
		}

		public async Task<DeadLine> AddDeadLine(
			string courseId,
			int groupId,
			Guid unitId,
			DeadLineSlideType slideType,
			string slideValue,
			List<Guid> userIds,
			DateTime date,
			int scorePercent)
		{
			var deadLine = new DeadLine
			{
				CourseId = courseId,
				GroupId = groupId,
				UnitId = unitId,
				SlideType = slideType,
				SlideValue = slideValue,
				UserIds = userIds,
				Date = date,
				ScorePercent = scorePercent
			};

			db.DeadLines.Add(deadLine);
			await db.SaveChangesAsync();

			return deadLine;
		}

		public async Task UpdateDeadLine(DeadLine deadLine)
		{
			db.DeadLines.Update(deadLine);
			await db.SaveChangesAsync();
		}

		public async Task DeleteDeadLine(DeadLine deadLine)
		{
			db.DeadLines.Remove(deadLine);
			await db.SaveChangesAsync();
		}
	}
}