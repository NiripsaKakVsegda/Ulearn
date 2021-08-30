using System;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class FlashcardStatistic
	{
		public FlashcardStatistic()
		{
			Statistics = new TotalRateResponse();
		}

		[DataMember]
		public string FlashcardId;

		[DataMember]
		public int VisitCount;

		[DataMember]
		public int UniqueVisitCount;

		[DataMember]
		public Guid UnitId;

		[DataMember]
		public string UnitTitle;

		[DataMember]
		public TotalRateResponse Statistics;
	}
}