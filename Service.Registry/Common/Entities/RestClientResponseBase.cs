using System.Net.Http;

namespace Service.Registry.Common.Entities
{
	public abstract class RestClientResponseBase
	{
		protected readonly RestClientResponseSettings Settings;
		protected readonly IHttpClientFactory ClientFactory;

		protected RestClientResponseBase(RestClientResponseSettings settings)
		{
			Settings = settings;
		}

		protected RestClientResponseBase(RestClientResponseSettings settings, IHttpClientFactory clientFactory)
		{
			Settings = settings;
			ClientFactory = clientFactory;
		}
	}
}
