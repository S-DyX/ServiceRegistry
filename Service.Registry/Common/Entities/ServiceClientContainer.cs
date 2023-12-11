using System.Collections.Generic;
using System.Linq;

namespace Service.Registry.Common.Entities
{
	

	public sealed class ServiceClientContainer
	{
		private Dictionary<string, List<ServiceClient>> RestClientDictionary { get; set; }

		public ServiceClientContainer()
		{
			RestClientDictionary = new Dictionary<string, List<ServiceClient>>();
		}

		public void AddRestClient(ServiceClient settings)
		{
			List<ServiceClient> clients;
			if (RestClientDictionary.TryGetValue(settings.Name, out clients))
			{
				clients.Add(settings);
			}
			else
			{
				RestClientDictionary.Add(settings.Name, new List<ServiceClient>() { settings });
			}
		}

		public void RemoveRestClient(ServiceClient settings)
		{
			List<ServiceClient> clients;
			if (RestClientDictionary.TryGetValue(settings.Name, out clients))
			{
				var client = clients.SingleOrDefault(a => a.Host == settings.Host && a.Name == settings.Name);
				if (client != null)
				{
					clients.Remove(client);
				}
			}
		}

		public ServiceClient GetRestClient(string name)
		{
			if (!RestClientDictionary.ContainsKey(name) || RestClientDictionary[name].Count <= 0)
				return null;
			var clients = RestClientDictionary[name];
			clients.Sort((left, right) => left.CallCount - right.CallCount);
			++clients.First().CallCount;
			return clients.First();

		}
		public List<ServiceClient> GetAllRestClient(string name)
		{
			if (!RestClientDictionary.ContainsKey(name) || RestClientDictionary[name].Count <= 0)
				return null;
			var clients = RestClientDictionary[name];
			return clients;

		}
		public List<ServiceClient> GetRestClients()
		{
			var result = new List<ServiceClient>();
			foreach (var clients in RestClientDictionary)
			{
				result.AddRange(clients.Value);
			}

			return result;
		}

		public void AddRestClients(List<ServiceClient> restClients)
		{
			foreach (var client in restClients)
			{
				List<ServiceClient> clientList;
				if (RestClientDictionary.TryGetValue(client.Name, out clientList))
				{
					if (clientList.FirstOrDefault(a => a.Host == client.Host) == null)
						clientList.Add(client);
				}
				else
				{
					RestClientDictionary.Add(client.Name, new List<ServiceClient> { client });
				}
			}
		}

		public void RemoveRestClients(List<ServiceClient> restClients)
		{
			foreach (var client in restClients)
			{
				if (!RestClientDictionary.ContainsKey(client.Name))
					continue;

				var clientList = RestClientDictionary[client.Name];
				var clientToRemove = clientList.FirstOrDefault(a => a.Host == client.Host);
				if (clientToRemove != null)
				{
					clientList.Remove(clientToRemove);
				}
			}
		}
	}
}
