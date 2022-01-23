using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public interface IAdditionalContentPublicationsRepo
	{
		Task<List<AdditionalContentPublication>> GetAdditionalContentPublications(string courseId, int groupId);
		Task<List<AdditionalContentPublication>> GetAdditionalContentPublications(string courseId, HashSet<int> groupIds);
		Task<AdditionalContentPublication> AddAdditionalContentPublication(string courseId, int groupId, string authorId, Guid unitId, Guid? slideId, DateTime date);
		Task UpdateAdditionalContentPublication(AdditionalContentPublication additionalContentPublication);
		Task DeleteAdditionalContentPublication(AdditionalContentPublication additionalContentPublication);
		Task<AdditionalContentPublication> GetAdditionalContentPublicationById(Guid additionalContentPublicationId);
		Task<bool> HasPublication(string courseId, int groupId, Guid unitId, Guid? slideId);
		
		Task<bool> IsSlidePublishedForGroups(string courseId, Slide slide, HashSet<int> userGroupIds);
	}
}