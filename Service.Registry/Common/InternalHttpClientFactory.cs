using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Service.Registry.Common
{
    internal class InternalHttpClientFactory : IHttpClientFactory
    {
        private readonly Dictionary<string, Queue<HttpClient>> _clients = new Dictionary<string, Queue<HttpClient>>(20);
        private readonly object _sync = new object();
        public HttpClient CreateClient(string name)
        {
            lock (_sync)
            {

                if (!_clients.ContainsKey(name))
                {
                    var handler = new HttpClientHandler();
                    handler.MaxConnectionsPerServer = 15;

                    var httpClients = new Queue<HttpClient>(20);

                    for (int i = 0; i < 20; i++)
                    {
                        var client = new HttpClient(handler)
                        {
                            Timeout = TimeSpan.FromSeconds(1),
                        };
                        httpClients.Enqueue(client);
                    }
                    _clients[name] = httpClients;
                }

                var queue = _clients[name];
                var result = queue.Dequeue();
                queue.Enqueue(result);
                return result;

            }

        }
    }
}
