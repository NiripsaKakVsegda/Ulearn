using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Database;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.Web.Api.Models.Parameters.Review;

public class ReviewQueueFilterParameters
{
	[FromQuery(Name = "courseId")]
	[Required]
	public string CourseId { get; set; }

	[FromQuery(Name = "studentsFilter")]
	public StudentsFilter StudentsFilter { get; set; } = StudentsFilter.MyGroups;

	[FromQuery(Name = "groupIds")]
	[CanBeNull]
	public List<int> GroupIds { get; set; }

	[FromQuery(Name = "studentIds")]
	[CanBeNull]
	public List<string> StudentIds { get; set; }

	[FromQuery(Name = "slideIds")]
	[CanBeNull]
	public List<Guid> SlideIds { get; set; }

	[FromQuery(Name = "sort")]
	public DateSort DateSort { get; set; } = DateSort.Ascending;

	[FromQuery(Name = "count")]
	[MinValue(0)]
	[MaxValue(1000)]
	public int Count { get; set; } = 500;
}

public class ReviewQueueHistoryFilterParameters : ReviewQueueFilterParameters
{
	[FromQuery(Name = "minTimestamp")]
	[CanBeNull]
	public DateTime? MinCheckedTimestamp { get; set; }
}

public enum StudentsFilter
{
	All,
	MyGroups,
	GroupIds,
	StudentIds
}