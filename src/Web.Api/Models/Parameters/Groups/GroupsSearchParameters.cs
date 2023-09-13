using System.ComponentModel.DataAnnotations;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Parameters;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.Web.Api.Models.Parameters.Groups;

public class GroupsSearchParameters : IPaginationParameters
{
	[FromQuery]
	[CanBeNull]
	public string CourseId { get; set; }

	[FromQuery]
	[CanBeNull]
	public GroupType? GroupType { get; set; }

	[FromQuery]
	public bool IncludeArchived { get; set; } = false;

	/// <summary>
	/// Для поиска групп, где указанный пользователь преподаватель или владелец
	/// </summary>
	[FromQuery]
	[CanBeNull]
	public string InstructorId { get; set; }

	[FromQuery]
	[CanBeNull]
	public string MemberId { get; set; }

	[FromQuery]
	[MaxLength(100, ErrorMessage = "Query should be at most 100 chars")]
	[CanBeNull]
	public string Query { get; set; }
	
	[FromQuery]
	[MinValue(0, ErrorMessage = "Offset should be non-negative")]
	public int Offset { get; set; } = 0;
	
	[FromQuery]
	[MinValue(0, ErrorMessage = "Count should be non-negative")]
	[MaxValue(500, ErrorMessage = "Count should be at most 500")]
	public int Count { get; set; } = 100;
}