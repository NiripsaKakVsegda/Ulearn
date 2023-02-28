using System;
using Ulearn.Core.Configuration;

namespace Ulearn.VideoAnnotations.Web.Configuration
{
	public class VideoAnnotationsConfiguration : UlearnConfiguration
	{
		public VideoAnnotationsConfigurationContent VideoAnnotations { get; set; }
	}

	public class VideoAnnotationsConfigurationContent
	{
		public string GoogleDocsApiKey { get; set; }

		public TimeSpan Timeout { get; set; }

		public CacheConfiguration Cache { get; set; } = new();
	}

	public class CacheConfiguration
	{
		public int Capacity { get; set; } = 50;

		public TimeSpan MaxLifeTime { get; set; } = TimeSpan.FromHours(1);
	}
}