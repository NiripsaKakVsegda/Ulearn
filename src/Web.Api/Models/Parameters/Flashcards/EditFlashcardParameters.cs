using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;

namespace Ulearn.Web.Api.Models.Parameters.Flashcards;

public class EditFlashcardParameters
{
	[FromBody]
	[MaxLength(2000)]
	[NotEmpty(CanBeNull = true)]
	[CanBeNull]
	public string Question { get; set; }

	[FromBody]
	[MaxLength(2000)]
	[NotEmpty(CanBeNull = true)]
	[CanBeNull]
	public string Answer { get; set; }
}