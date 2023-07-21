using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards;

[DataContract]
public class CreateFlashcardResponse
{
	[DataMember]
	public string Id { get; set; }
}