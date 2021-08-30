using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public class UserFlashcardStatistics
	{
		public UserFlashcardStatistics()
		{
			UnitUserStatistics = new List<UnitUserStatistic>();
		}

		[DataMember]
		public string UserId;

		[DataMember]
		public string UserName;

		[DataMember]
		public string GroupName;

		[DataMember]
		public int GroupId;

		[DataMember]
		public List<UnitUserStatistic> UnitUserStatistics;
	}
}