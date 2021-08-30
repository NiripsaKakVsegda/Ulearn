using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Ulearn.Common.Api;
using Ulearn.Core.Configuration;
using Ulearn.VideoAnnotations.Web.Annotations;
using Ulearn.VideoAnnotations.Web.Configuration;
using Vostok.Hosting.Abstractions;

namespace Ulearn.VideoAnnotations.Web
{
	public class WebApplication : BaseApiWebApplication
	{
		private readonly VideoAnnotationsConfiguration configuration;

		public WebApplication()
		{
			configuration = ApplicationConfiguration.Read<VideoAnnotationsConfiguration>();
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
		{
			base.ConfigureServices(services, hostingEnvironment);

			services.Configure<VideoAnnotationsConfiguration>(options => options.SetFrom(hostingEnvironment.SecretConfigurationProvider.Get<VideoAnnotationsConfiguration>(hostingEnvironment.SecretConfigurationSource)));

			services.AddSwaggerExamplesFromAssemblyOf<WebApplication>();
		}

		public override void ConfigureDi(IServiceCollection services)
		{
			base.ConfigureDi(services);

			services.AddSingleton<IAnnotationsParser, AnnotationsParser>();
			services.AddSingleton<IGoogleDocApiClient, GoogleDocApiClient>();
			services.AddSingleton<IAnnotationsCache, AnnotationsCache>();
		}
	}
}