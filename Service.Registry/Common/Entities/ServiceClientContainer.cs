using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Registry.Common.Entities
{
	/// <summary>
	/// Thread-safe storage of registered clients
	/// </summary>
	public sealed class ServiceClientContainer
	{
		private readonly Dictionary<string, List<ServiceClient>> _clients =
			new Dictionary<string, List<ServiceClient>>(StringComparer.Ordinal);
		private readonly object _sync = new object();

		public void AddRestClient(ServiceClient settings)
		{
			if (!IsValid(settings))
				return;
			lock (_sync)
				AddUnsafe(settings);
		}

		public void RemoveRestClient(ServiceClient settings)
		{
			if (!IsValid(settings))
				return;
			lock (_sync)
				RemoveUnsafe(settings);
		}

		/// <summary>
		/// Returns the least used client and increments its call counter
		/// </summary>
		public ServiceClient GetRestClient(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			lock (_sync)
			{
				if (!_clients.TryGetValue(name, out var clients) || clients.Count == 0)
					return null;

				var selected = clients[0];
				for (var i = 1; i < clients.Count; i++)
				{
					if (clients[i].CallCount < selected.CallCount)
						selected = clients[i];
				}

				if (selected.CallCount == int.MaxValue)
				{
					// all counters reached the limit
					foreach (var client in clients)
						client.CallCount = 0;
				}

				selected.CallCount++;
				return Copy(selected);
			}
		}

		public List<ServiceClient> GetAllRestClient(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			lock (_sync)
			{
				if (!_clients.TryGetValue(name, out var clients) || clients.Count == 0)
					return null;
				return clients.Select(Copy).ToList();
			}
		}

		public List<ServiceClient> GetRestClients()
		{
			lock (_sync)
			{
				return _clients.Values.SelectMany(x => x).Select(Copy).ToList();
			}
		}

		public void AddRestClients(List<ServiceClient> restClients)
		{
			if (restClients == null)
				return;

			lock (_sync)
			{
				foreach (var client in restClients.Where(IsValid))
					AddUnsafe(client);
			}
		}

		public void RemoveRestClients(List<ServiceClient> restClients)
		{
			if (restClients == null)
				return;

			lock (_sync)
			{
				foreach (var client in restClients.Where(IsValid))
					RemoveUnsafe(client);
			}
		}

		private void AddUnsafe(ServiceClient client)
		{
			if (_clients.TryGetValue(client.Name, out var list))
			{
				if (!list.Any(x => IsSame(x, client)))
					list.Add(Copy(client));
			}
			else
			{
				_clients.Add(client.Name, new List<ServiceClient> { Copy(client) });
			}
		}

		private void RemoveUnsafe(ServiceClient client)
		{
			if (!_clients.TryGetValue(client.Name, out var list))
				return;

			list.RemoveAll(x => IsSame(x, client));
			if (list.Count == 0)
				_clients.Remove(client.Name);
		}

		private static bool IsValid(ServiceClient client)
		{
			return !string.IsNullOrEmpty(client?.Name);
		}

		private static bool IsSame(ServiceClient left, ServiceClient right)
		{
			return string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase)
				   && string.Equals(left.Address, right.Address, StringComparison.OrdinalIgnoreCase)
				   && left.Port == right.Port;
		}

		private static ServiceClient Copy(ServiceClient client)
		{
			return new ServiceClient
			{
				Name = client.Name,
				Host = client.Host,
				Address = client.Address,
				Port = client.Port,
				CallCount = client.CallCount
			};
		}
	}
}
