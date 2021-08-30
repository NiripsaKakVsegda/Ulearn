using System.Runtime.Serialization;
using Ulearn.Common.Extensions;

namespace XQueue.Models
{
	[DataContract]
	public class XQueueResult
	{
		[DataMember(Name = "xqueue_header")]
		public string header { get; set; }

		[DataMember(Name = "xqueue_body")]
		public string body { get; set; }

		[IgnoreDataMember]
		public XQueueHeader Header
		{
			get => header.DeserializeJson<XQueueHeader>();
			set => header = value.JsonSerialize();
		}

		[IgnoreDataMember]
		public XQueueResultBody Body
		{
			get => body.DeserializeJson<XQueueResultBody>();
			set => body = value.JsonSerialize();
		}
	}

	[DataContract]
	public class XQueueResultBody
	{
		[DataMember(Name = "correct")]
		public bool IsCorrect { get; set; }

		[DataMember(Name = "score")]
		public double Score { get; set; }

		[DataMember(Name = "msg")]
		public string Message { get; set; }
	}
}