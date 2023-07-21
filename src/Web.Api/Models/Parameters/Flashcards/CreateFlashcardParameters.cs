using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.Web.Api.Models.Parameters.Flashcards;

public class CreateFlashcardParameters
{
	[FromBody]
	[Required]
	public string CourseId { get; set; } = null!;

	[FromBody]
	[Required]
	public Guid UnitId { get; set; }

	[FromBody]
	[NotEmpty]
	[MaxLength(2000)]
	public string Question { get; set; } = null!;

	[FromBody]
	[NotEmpty]
	[MaxLength(2000)]
	public string Answer { get; set; } = null!;

	/// <summary>
	/// Moderators can create approved flashcards 
	/// </summary>
	[FromBody]
	[CanBeNull]
	public bool? Approved { get; set; }
}