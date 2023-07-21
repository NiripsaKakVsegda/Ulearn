using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class UnitFlashcardsResponse
	{
		public UnitFlashcardsResponse()
		{
			Flashcards = new List<BaseFlashcardResponse>();
		}

		[DataMember]
		public Guid UnitId;

		[DataMember]
		public string UnitTitle;

		[DataMember]
		public bool Unlocked;

		[DataMember]
		public List<BaseFlashcardResponse> Flashcards;
	}
}