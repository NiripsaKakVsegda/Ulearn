using System;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses.Manager;
using Ulearn.Web.Api.Models.Responses.Review;

namespace Ulearn.Web.Api.Controllers.Review
{
	[Route("/favourite-reviews")]
	public class FavouriteReviewsController : BaseController
	{
		private readonly IFavouriteReviewsRepo favouriteReviewsRepo;
		private readonly IUnitsRepo unitsRepo;

		public FavouriteReviewsController(
			ICourseStorage courseStorage,
			UlearnDb db,
			IFavouriteReviewsRepo favouriteReviewsRepo,
			IUnitsRepo unitsRepo,
			IUsersRepo usersRepo)
			: base(courseStorage, db, usersRepo)
		{
			this.favouriteReviewsRepo = favouriteReviewsRepo;
			this.unitsRepo = unitsRepo;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var courseId = (string)context.ActionArguments["courseId"];
			var slideId = (Guid)context.ActionArguments["slideId"];

			var course = courseStorage.FindCourse(courseId);
			if (course == null)
			{
				context.Result = NotFound(new ErrorResponse("Course not found"));
				return;
			}

			var visibleUnitsIds = await unitsRepo.GetVisibleUnitIds(course, UserId);
			var slide = course.FindSlideById(slideId, true, visibleUnitsIds);
			if (slide == null)
			{
				context.Result = NotFound(new ErrorResponse("Slide not found"));
				return;
			}

			await next();
		}

		[HttpGet]
		[Authorize(Policy = "Instructors")]
		[SwaggerResponse((int)HttpStatusCode.Forbidden, "You don't have access to view submissions")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Course not found")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Slide not found")]
		public async Task<ActionResult<FavouriteReviewsResponse>> GetFavouriteReviews([FromQuery] string courseId, [FromQuery] Guid slideId)
		{
			var startDate = DateTime.Now.AddYears(-1).AddMonths(-6);

			var favouriteReviews = await favouriteReviewsRepo.GetFavouriteReviewsForUser(courseId, slideId, UserId);
			var favouriteReviewsForOthersUsers = await favouriteReviewsRepo.GetFavouriteReviewsForOtherUsers(courseId, slideId, UserId, startDate);

			return FavouriteReviewsResponse.Build(favouriteReviews, favouriteReviewsForOthersUsers);
		}

		[HttpPost]
		[Authorize(Policy = "Instructors")]
		[SwaggerResponse((int)HttpStatusCode.Forbidden, "You don't have access to view submissions")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Course not found")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Slide not found")]
		public async Task<ActionResult<FavouriteReviewResponse>> AddFavouriteReview([FromQuery] string courseId, [FromQuery] Guid slideId, [FromBody] string text)
		{
			var favouriteReview = await favouriteReviewsRepo.AddFavouriteReviewByUser(courseId, slideId, UserId, text);
			return FavouriteReviewResponse.Build(favouriteReview);
		}

		[HttpDelete]
		[Authorize(Policy = "Instructors")]
		[SwaggerResponse((int)HttpStatusCode.Forbidden, "You don't have access to view submissions")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Course not found")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Slide not found")]
		[SwaggerResponse((int)HttpStatusCode.NotFound, "Favourite review with id {favouriteReviewId} not found")]
		public async Task<ActionResult> DeleteFavouriteReview([FromQuery] string courseId, [FromQuery] Guid slideId, [FromQuery] int favouriteReviewId)
		{
			var favouriteReview = await favouriteReviewsRepo.FindFavouriteReviewByUser(favouriteReviewId);
			if (favouriteReview == null)
				return NotFound(new ErrorResponse($"Favourite review with id {favouriteReviewId} not found"));

			await favouriteReviewsRepo.DeleteFavouriteReviewByUser(favouriteReview);

			return Ok("Favourite review deleted");
		}
	}
}