using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class UserFlashcardStatisticResponse
	{
		public UserFlashcardStatisticResponse()
		{
			UsersFlashcardsStatistics = new List<UserFlashcardStatistics>();
		}

		[DataMember]
		public List<UserFlashcardStatistics> UsersFlashcardsStatistics;
	}
}