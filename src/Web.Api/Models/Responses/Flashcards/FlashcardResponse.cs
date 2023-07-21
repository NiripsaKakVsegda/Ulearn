using System;
using System.Runtime.Serialization;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Flashcards
{
	[DataContract]
	public abstract class BaseFlashcardResponse
	{
		[DataMember]
		public abstract FlashcardType FlashcardType { get; set; }

		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string CourseId { get; set; }

		[DataMember]
		public Guid UnitId { get; set; }

		[DataMember]
		public string Question { get; set; }

		[DataMember]
		public string Answer { get; set; }

		[DataMember]
		public Rate Rate { get; set; }

		[DataMember]
		public int LastRateIndex { get; set; }
	}

	[DataContract]
	public class CourseFlashcardResponse : BaseFlashcardResponse
	{
		[DataMember]
		public override FlashcardType FlashcardType { get; set; } = FlashcardType.CourseFlashcard;

		[DataMember]
		public Guid[] TheorySlidesIds { get; set; }
	}

	[DataContract]
	public class UserGeneratedFlashcardResponse : BaseFlashcardResponse
	{
		[DataMember]
		public override FlashcardType FlashcardType { get; set; } = FlashcardType.UserFlashcard;

		[DataMember]
		public bool IsPublished { get; set; }

		[DataMember]
		[CanBeNull]
		public ShortUserInfo Owner { get; set; }

		[DataMember]
		[CanBeNull]
		public DateTime? LastUpdateTimestamp { get; set; }

		[DataMember]
		[CanBeNull]
		public FlashcardModerationStatus? ModerationStatus { get; set; }

		[DataMember]
		[CanBeNull]
		public ShortUserInfo Moderator { get; set; }

		[DataMember]
		[CanBeNull]
		public DateTime? ModerationTimestamp { get; set; }
	}

	public enum FlashcardType
	{
		UserFlashcard,
		CourseFlashcard
	}
}