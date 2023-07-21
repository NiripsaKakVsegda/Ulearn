using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards;

[DataContract]
public class UserGeneratedFlashcardsResponse
{
	[DataMember]
	public List<UserGeneratedFlashcardResponse> Flashcards { get; set; }
}