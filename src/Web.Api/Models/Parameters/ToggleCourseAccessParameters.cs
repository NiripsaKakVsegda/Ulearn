using System.ComponentModel.DataAnnotations;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.Web.Api.Models.Parameters;

public class ToggleCourseAccessParameters
{
	[FromBody]
	[Required]
	public string UserId { get; set; }

	[FromBody]
	[Required]
	public CourseAccessType AccessType { get; set; }

	[FromBody]
	[Required(AllowEmptyStrings = true)]
	public string Comment { get; set; }
}