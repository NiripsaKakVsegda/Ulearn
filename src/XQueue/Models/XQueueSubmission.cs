using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Extensions;

namespace XQueue.Models
{
	[DataContract]
	public class XQueueResponse
	{
		[DataMember(Name = "return_code")]
		public int ReturnCode { get; set; }

		[DataMember(Name = "content")]
		public string Content { get; set; }
	}

	[DataContract]
	public class XQueueSubmission
	{
		[DataMember(Name = "xqueue_header")]
		public string header { get; set; }

		[DataMember(Name = "xqueue_files")]
		public Dictionary<string, string> Files { get; set; }

		[DataMember(Name = "xqueue_body")]
		public string body { get; set; }

		[IgnoreDataMember]
		public XQueueHeader Header
		{
			get => header.DeserializeJson<XQueueHeader>();
			set => header = value.JsonSerialize();
		}

		[IgnoreDataMember]
		public XQueueSubmissionBody Body
		{
			get => body.DeserializeJson<XQueueSubmissionBody>();
			set => body = value.JsonSerialize();
		}
	}

	[DataContract]
	public class XQueueHeader
	{
		[DataMember(Name = "submission_id")]
		public string SubmissionId { get; set; }

		[DataMember(Name = "submission_key")]
		public string SubmissionKey { get; set; }
	}

	[DataContract]
	public class XQueueSubmissionBody
	{
		[DataMember(Name = "student_info")]
		public XQueueStudentInfo StudentInfo { get; set; }

		[DataMember(Name = "student_response")]
		public string StudentResponse { get; set; }

		[DataMember(Name = "grader_payload")]
		public string GraderPayload { get; set; }
	}

	[DataContract]
	public class XQueueStudentInfo
	{
		[DataMember(Name = "anonymous_student_id")]
		public string AnonymousStudentId { get; set; }

		[DataMember(Name = "submission_time")]
		public string SubmissionTime { get; set; }

		[DataMember(Name = "random_seed")]
		public string RandomSeed { get; set; }
	}
}