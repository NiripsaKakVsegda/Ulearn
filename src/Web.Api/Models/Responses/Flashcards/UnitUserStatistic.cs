using System;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class UnitUserStatistic
	{
		[DataMember]
		public Guid UnitId;

		[DataMember]
		public string UnitTitle;

		[DataMember]
		public int TotalFlashcardsCount;

		[DataMember]
		public int TotalFlashcardVisits;

		[DataMember]
		public int UniqueFlashcardVisits;

		[DataMember]
		public int Rate5Count;
	}
}