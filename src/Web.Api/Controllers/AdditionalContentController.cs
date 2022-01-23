using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/additional-content")]
	public class AdditionalContentPublicationsController : BaseController
	{
		private IAdditionalContentPublicationsRepo additionalContentPublicationsRepo;
		private IGroupsRepo groupsRepo;

		public AdditionalContentPublicationsController(ICourseStorage courseStorage, UlearnDb db,
			IUsersRepo usersRepo,
			IGroupsRepo groupsRepo,
			IAdditionalContentPublicationsRepo additionalContentPublicationsRepo
		)
			: base(courseStorage, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.additionalContentPublicationsRepo = additionalContentPublicationsRepo;
		}

		[HttpGet]
		[Route("{courseId}")]
		public async Task<ActionResult<AdditionalContentPublicationsResponse>> GetAdditionalContentPublications([FromRoute] string courseId, [FromQuery] int groupId)
		{
			var publications = await additionalContentPublicationsRepo.GetAdditionalContentPublications(courseId, groupId);
			var userIds = publications.Select(p => p.AuthorId).Distinct().ToList();
			var users = (await usersRepo.GetUsersByIds(userIds)).ToDictionary(u => u.Id, u => u);

			return new AdditionalContentPublicationsResponse
			{
				Publications = publications
					.Select(p => AdditionalContentPublicationResponse.Build(p, users[p.AuthorId]))
					.ToList()
			};
		}

		[HttpPost]
		[Route("{courseId}")]
		public async Task<ActionResult<AdditionalContentPublicationResponse>> AddAdditionalContentPublication(
			[FromRoute] string courseId,
			[FromQuery] int groupId,
			[FromQuery] Guid unitId,
			[FromQuery] Guid? slideId,
			[FromQuery] DateTime date
		)
		{
			var course = courseStorage.FindCourse(courseId);
			if (course == null)
			{
				return NotFound();
			}

			var unit = course.GetUnits(new[] { unitId }).FirstOrDefault();
			if (unit == null)
			{
				return NotFound();
			}

			if (slideId != null)
			{
				var slides = unit.GetSlides(false);
				if (slides.All(s => s.Id != slideId))
				{
					return NotFound();
				}
			}
			
			var group = (await groupsRepo.GetCourseGroupsQueryable(courseId).Where(g => g.Id == groupId).ToListAsync()).FirstOrDefault();

			if (group == null)
			{
				return NotFound();
			}

			if (await additionalContentPublicationsRepo.HasPublication(courseId, groupId, unitId, slideId))
			{
				return UnprocessableEntity($"There's already a publication for this content, try update it instead");
			}

			var publication = await additionalContentPublicationsRepo.AddAdditionalContentPublication(courseId, groupId, UserId, unitId, slideId, date);
			var user = (await usersRepo.GetUsersByIds(new[] { UserId })).FirstOrDefault();

			return AdditionalContentPublicationResponse.Build(publication, user);
		}

		[HttpPatch]
		public async Task<ActionResult> UpdateAdditionalContentPublication([FromQuery] Guid publicationId, [FromQuery] DateTime date)
		{
			var publicationToUpdate = await additionalContentPublicationsRepo.GetAdditionalContentPublicationById(publicationId);
			if (publicationToUpdate == null)
			{
				return NotFound($"Publication with id {publicationId} not found");
			}

			publicationToUpdate.Date = date;

			await additionalContentPublicationsRepo.UpdateAdditionalContentPublication(publicationToUpdate);

			return NoContent();
		}

		[HttpDelete]
		public async Task<ActionResult> DeleteAdditionalContentPublication([FromQuery] Guid publicationId)
		{
			var publication = await additionalContentPublicationsRepo.GetAdditionalContentPublicationById(publicationId);
			if (publication == null)
			{
				return NotFound($"Publication with id {publicationId} not found");
			}

			await additionalContentPublicationsRepo.DeleteAdditionalContentPublication(publication);

			return Ok($"Publication with id {publicationId} deleted");
		}
	}

	[DataContract]
	public class AdditionalContentPublicationsResponse
	{
		[DataMember]
		public List<AdditionalContentPublicationResponse> Publications;
	}

	[DataContract]
	public class AdditionalContentPublicationResponse
	{
		[DataMember]
		public Guid Id;

		[DataMember]
		public string CourseId;

		[DataMember]
		public int GroupId;

		[DataMember]
		public Guid UnitId;

		[DataMember]
		[CanBeNull]
		public Guid? SlideId;

		[DataMember]
		public DateTime Date;

		[DataMember]
		public ShortUserInfo Author;

		public static AdditionalContentPublicationResponse Build(AdditionalContentPublication p, ApplicationUser user)
		{
			return new AdditionalContentPublicationResponse
			{
				Id = p.Id,
				Author = BaseController.BuildShortUserInfo(user),
				Date = p.Date,
				UnitId = p.UnitId,
				SlideId = p.SlideId,
				CourseId = p.CourseId,
				GroupId = p.GroupId,
			};
		}
	}
}