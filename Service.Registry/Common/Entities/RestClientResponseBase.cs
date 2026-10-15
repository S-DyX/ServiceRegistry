using System;
using System.Net.Http;

namespace Service.Registry.Common.Entities
{
	public abstract class RestClientResponseBase
	{
		protected readonly RestClientResponseSettings Settings;
		protected readonly IHttpClientFactory ClientFactory;

		/// <summary>
		/// Ctor, <see cref="ClientFactory"/> is the shared internal factory
		/// </summary>
		protected RestClientResponseBase(RestClientResponseSettings settings)
			: this(settings, null)
		{
		}

		protected RestClientResponseBase(RestClientResponseSettings settings, IHttpClientFactory clientFactory)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
			ClientFactory = clientFactory ?? InternalHttpClientFactory.Default;
		}
	}
}
