using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;

namespace Database.Utils;

public static class AdditionalContentPublicationUtils
{
	public static async Task<(
		Dictionary<Guid, AdditionalContentPublication> unitsPublications,
		Dictionary<Guid, AdditionalContentPublication> slidesPublications
		)> GetPublications(
		IGroupMembersRepo groupMembersRepo,
		IAdditionalContentPublicationsRepo additionalContentPublicationsRepo,
		string courseId,
		string userId = null)
	{
		var unitsPublications = new Dictionary<Guid, AdditionalContentPublication>();
		var slidesPublications = new Dictionary<Guid, AdditionalContentPublication>();

		if (userId == null)
			return (unitsPublications, slidesPublications);

		var userGroups = await groupMembersRepo.GetUserGroupsAsync(courseId, userId);

		if (userGroups.Count == 0)
			return (unitsPublications, slidesPublications);

		var publications = await additionalContentPublicationsRepo.GetAdditionalContentPublications(courseId, userGroups.Select(g => g.Id)
			.ToHashSet());

		foreach (var publication in publications)
		{
			if (publication.SlideId.HasValue)
			{
				var slideId = publication.SlideId.Value;
				if (slidesPublications.ContainsKey(slideId))
					slidesPublications[slideId] = slidesPublications[slideId].Date <= publication.Date
						? slidesPublications[slideId]
						: publication;
				else
					slidesPublications.Add(slideId, publication);
			}
			else
			{
				if (unitsPublications.ContainsKey(publication.UnitId))
					unitsPublications[publication.UnitId] = unitsPublications[publication.UnitId].Date <= publication.Date
						? unitsPublications[publication.UnitId]
						: publication;
				else
					unitsPublications.Add(publication.UnitId, publication);
			}
		}

		return (unitsPublications, slidesPublications);
	}
}