using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

namespace Service.Registry.Common
{
    /// <summary>
    /// <see cref="IHttpClientFactory"/> used when no factory is supplied.
    /// One thread-safe <see cref="HttpClient"/> per name; pooled connections are recycled
    /// periodically so DNS / address changes are picked up.
    /// </summary>
    internal sealed class InternalHttpClientFactory : IHttpClientFactory
    {
        private static readonly Lazy<InternalHttpClientFactory> DefaultInstance =
            new Lazy<InternalHttpClientFactory>(() => new InternalHttpClientFactory(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly ConcurrentDictionary<string, Lazy<HttpClient>> _clients =
            new ConcurrentDictionary<string, Lazy<HttpClient>>(StringComparer.Ordinal);

        /// <summary>
        /// Shared instance
        /// </summary>
        public static InternalHttpClientFactory Default => DefaultInstance.Value;

        public HttpClient CreateClient(string name)
        {
            return _clients.GetOrAdd(name ?? string.Empty,
                _ => new Lazy<HttpClient>(Create, LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        private static HttpClient Create()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                MaxConnectionsPerServer = 64
            };

            // Requests to the registry itself use their own short timeout (CancellationToken),
            // REST clients may transfer large files, so the client timeout is long.
            return new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }
    }
}
