using Web.Api.Configuration;

namespace uLearn.Web.Core.Utils;

public interface IConfigBuilder<in T>
{
	void Build(WebConfiguration configuration, T options);
}