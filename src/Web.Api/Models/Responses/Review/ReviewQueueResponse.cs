using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Review
{
	[DataContract]
	public class ReviewQueueResponse : SuccessResponse
	{
		[DataMember]
		public List<ReviewQueueItem> Checkings { get; set; }
	}

	[DataContract]
	public class ReviewQueueMetaResponse : SuccessResponse
	{
		[DataMember]
		public List<ShortReviewQueueItem> Checkings { get; set; }
	}

	[DataContract]
	public class ReviewQueueItem
	{
		[DataMember]
		public QueueItemType Type { get; set; }

		[DataMember]
		public int SubmissionId { get; set; }

		[DataMember]
		public Guid SlideId { get; set; }

		[DataMember]
		public ShortUserInfo User { get; set; } = null!;

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		[CanBeNull]
		public int? Score { get; set; }

		[DataMember]
		public int MaxScore { get; set; }

		[DataMember]
		[CanBeNull]
		public ShortUserInfo LockedBy { get; set; }

		[DataMember]
		[CanBeNull]
		public DateTime? LockedUntil { get; set; }

		[DataMember]
		[CanBeNull]
		public DateTime? CheckedTimestamp { get; set; }

		[DataMember]
		[CanBeNull]
		public ShortUserInfo CheckedBy { get; set; }

		[DataMember]
		[CanBeNull]
		public List<ShortReviewInfo> Reviews { get; set; }
	}

	public class ShortReviewInfo
	{
		[DataMember]
		public int CommentId { get; set; }

		[DataMember]
		public ShortUserInfo Author { get; set; }

		[DataMember]
		public string CodeFragment { get; set; }

		[DataMember]
		public string Comment { get; set; }
	}

	public enum QueueItemType
	{
		Exercise,
		Quiz,
	}

	[DataContract]
	public class ShortReviewQueueItem
	{
		[DataMember]
		public int SubmissionId { get; set; }

		[DataMember]
		public Guid SlideId { get; set; }

		[DataMember]
		public string UserId { get; set; }

		[DataMember]
		[CanBeNull]
		public string LockedById { get; set; }

		[DataMember]
		[CanBeNull]
		public DateTime? LockedUntil { get; set; }
	}
}