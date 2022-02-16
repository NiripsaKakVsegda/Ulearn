using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public interface IDeadLinesRepo
	{
		Task<DeadLine> GetDeadLineById(Guid deadLineId);

		Task<List<DeadLine>> GetDeadLines(
			string courseId,
			HashSet<int> groupIds,
			[CanBeNull] Slide slide = null,
			[CanBeNull] Guid? userId = null);

		Task<List<DeadLine>> GetDeadLines(
			string courseId,
			int groupId,
			[CanBeNull] Slide slide = null,
			[CanBeNull] Guid? userId = null
		);

		Task<List<DeadLine>> GetDeadLinesForUser(
			string courseId,
			Guid userId,
			[CanBeNull] Slide slide = null
		);

		Task<DeadLine> AddDeadLine(
			string courseId,
			int groupId,
			Guid unitId,
			DeadLineSlideType slideType,
			string slideValue,
			List<Guid> userIds,
			DateTime date,
			int scorePercent);

		Task UpdateDeadLine(DeadLine deadLine);
		Task DeleteDeadLine(DeadLine deadLine);
	}
}