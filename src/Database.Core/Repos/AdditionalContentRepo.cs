using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public class AdditionalContentPublicationsRepo : IAdditionalContentPublicationsRepo
	{
		private readonly UlearnDb db;

		public AdditionalContentPublicationsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<List<AdditionalContentPublication>> GetAdditionalContentPublications(string courseId, int groupId)
		{
			return await GetAdditionalContentPublications(courseId, new HashSet<int> { groupId });
		}
		
		public async Task<List<AdditionalContentPublication>> GetAdditionalContentPublicationsForUser(string courseId, string userId)
		{
			var groups = db.Groups
				.Where(g => g.CourseId == courseId && !g.IsDeleted && !g.IsArchived)
				.Select(g => g.Id);
			var userGroupsIds = await db.GroupMembers
				.Where(m => groups.Contains(m.GroupId) && m.UserId == userId)
				.Select(m => m.GroupId)
				.ToListAsync();
			
			return await GetAdditionalContentPublications(courseId, userGroupsIds.ToHashSet());
		}

		public async Task<List<AdditionalContentPublication>> GetAdditionalContentPublications(string courseId, HashSet<int> groupIds)
		{
			return await db.AdditionalContentPublications
				.Where(ac => ac.CourseId == courseId && groupIds.Contains(ac.GroupId))
				.ToListAsync();
		}
		
		public async Task<AdditionalContentPublication> AddAdditionalContentPublication(string courseId, int groupId, string authorId, Guid unitId, Guid? slideId, DateTime date)
		{
			var content = new AdditionalContentPublication
			{
				CourseId = courseId,
				GroupId = groupId,
				AuthorId = authorId,
				UnitId = unitId,
				SlideId = slideId,
				Date = date,
			};

			db.AdditionalContentPublications.Add(content);
			await db.SaveChangesAsync();

			return content;
		}

		public async Task UpdateAdditionalContentPublication(AdditionalContentPublication additionalContentPublication)
		{
			db.AdditionalContentPublications.Update(additionalContentPublication);
			await db.SaveChangesAsync();
		}

		public async Task DeleteAdditionalContentPublication(AdditionalContentPublication additionalContentPublication)
		{
			db.AdditionalContentPublications.Remove(additionalContentPublication);
			await db.SaveChangesAsync();
		}

		public async Task<AdditionalContentPublication> GetAdditionalContentPublicationById(Guid additionalContentId)
		{
			return await db.AdditionalContentPublications.FindAsync(additionalContentId);
		}

		public async Task<bool> HasPublication(string courseId, int groupId, Guid unitId, Guid? slideId)
		{
			return await db.AdditionalContentPublications
				.Where(p => p.CourseId == courseId && p.GroupId == groupId && p.UnitId == unitId && p.SlideId == slideId)
				.AnyAsync();
		}

		public async Task<bool> IsSlidePublishedForUser(string courseId, Slide slide, string userId)
		{
			if (!slide.IsExtraContent && !slide.Unit.Settings.IsExtraContent)
				return true;
			
			var groups = db.Groups
				.Where(g => g.CourseId == courseId && !g.IsDeleted && !g.IsArchived)
				.Select(g => g.Id);
			var userGroupsIds = await db.GroupMembers
				.Where(m => groups.Contains(m.GroupId) && m.UserId == userId)
				.Select(m => m.GroupId)
				.ToListAsync();

			return await IsSlidePublishedForGroups(courseId, slide, userGroupsIds.ToHashSet());
		}

		public async Task<bool> IsSlidePublishedForGroups(string courseId, Slide slide, HashSet<int> userGroupIds)
		{
			if (!slide.IsExtraContent && !slide.Unit.Settings.IsExtraContent)
				return true;
			
			var unitPublications = await db.AdditionalContentPublications
				.Where(ac => ac.CourseId == courseId  && ac.UnitId == slide.Unit.Id && userGroupIds.Contains(ac.GroupId))
				.ToListAsync();
			var unitPublication = unitPublications.FirstOrDefault(p => p.SlideId == null);
			var slidePublication = unitPublications.FirstOrDefault(p => p.SlideId == slide.Id);

			if (slide.Unit.Settings.IsExtraContent && slide.IsExtraContent)
				return unitPublication != null &&
						slidePublication != null &&
						DateTime.Now >= unitPublication.Date &&
						DateTime.Now >= slidePublication.Date;

			if (slide.Unit.Settings.IsExtraContent)
				return unitPublication != null &&
						DateTime.Now >= unitPublication.Date;

			return slidePublication != null &&
					DateTime.Now >= slidePublication.Date;
		}
	}
}