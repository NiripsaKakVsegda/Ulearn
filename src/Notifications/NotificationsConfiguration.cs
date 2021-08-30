using Ulearn.Core.Configuration;

namespace Notifications
{
	public class NotificationsConfiguration : UlearnConfiguration
	{
		public bool Enabled { get; set; }
		public string SecretForHashes { get; set; }
		public KonturSpamConfiguration Spam { get; set; }
	}

	public class KonturSpamConfiguration
	{
		public string Endpoint { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public KonturSpamChannels Channels { get; set; }
		public KonturSpamTemplates Templates { get; set; }
	}

	public class KonturSpamChannels
	{
		public string Notifications { get; set; }
	}

	public class KonturSpamTemplates
	{
		public string WithButton { get; set; }
	}
}