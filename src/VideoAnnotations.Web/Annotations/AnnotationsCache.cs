using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.VideoAnnotations.Api.Models.Responses.Annotations;
using Ulearn.VideoAnnotations.Web.Configuration;

namespace Ulearn.VideoAnnotations.Web.Annotations
{
	public class AnnotationsCache : LruCache<string, Dictionary<string, Annotation>>, IAnnotationsCache
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(AnnotationsCache));

		public AnnotationsCache(IOptions<VideoAnnotationsConfiguration> options)
			: base(options.Value.VideoAnnotations.Cache.Capacity, options.Value.VideoAnnotations.Cache.MaxLifeTime)
		{
			var capacity = options.Value.VideoAnnotations.Cache.Capacity;
			var maxLifeTime = options.Value.VideoAnnotations.Cache.MaxLifeTime;
			log.Info("Creating annotations cache with capacity {capacity} and max life time {maxLifeTime}", capacity, maxLifeTime);
		}
	}
}