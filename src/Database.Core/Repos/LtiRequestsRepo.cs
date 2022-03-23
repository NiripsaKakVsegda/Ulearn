using System;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;

namespace Database.Repos
{
	public class LtiRequestsRepo : ILtiRequestsRepo
	{
		private readonly UlearnDb db;

		public LtiRequestsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task Update(string courseId, string userId, Guid slideId, string ltiRequestJson)
		{
			await FuncUtils.TrySeveralTimesAsync(async() => await TryUpdate(courseId, slideId, userId, ltiRequestJson), 3);
		}

		private async Task TryUpdate(string courseId, Guid slideId, string userId, string ltiRequestJson)
		{
			var ltiRequestModel = await FindElement(courseId, slideId, userId);
			if (ltiRequestModel == null)
			{
				ltiRequestModel = new LtiSlideRequest
				{
					CourseId = courseId,
					SlideId = slideId,
					UserId = userId,
					Request = ltiRequestJson
				};
			}
			else
				ltiRequestModel.Request = ltiRequestJson;

			db.AddOrUpdate(ltiRequestModel, r => r.RequestId == ltiRequestModel.RequestId);
			await db.SaveChangesAsync();
		}

		public async Task<string> Find(string courseId, string userId, Guid slideId)
		{
			var ltiRequestModel = await FindElement(courseId, slideId, userId);
			return ltiRequestModel?.Request;
		}

		private async Task<LtiSlideRequest> FindElement(string courseId, Guid slideId, string userId)
		{
			return await db.LtiRequests.FirstOrDefaultAsync(
				request => request.CourseId == courseId && request.UserId == userId && request.SlideId == slideId
			);
		}
	}
}