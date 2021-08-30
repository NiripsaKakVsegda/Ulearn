using System;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class FlashcardsUnitInfo
	{
		[DataMember]
		public string UnitTitle;

		[DataMember]
		public bool Unlocked;

		[DataMember]
		public int CardsCount;

		[DataMember]
		public Guid UnitId;
	}
}