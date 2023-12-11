using Service.Registry.Common.Entities;
using Service.Registry.Impl.Settings;
using Service.Registry.Interfaces.Ver001;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Registry.Impl.Services.Ver001
{
	public sealed class ServiceRegistryClient : IServiceRegistry
	{

		private readonly ClientSettings _clientSettings;
		private readonly ServiceClientContainer _serviceClientContainer;

		public ServiceRegistryClient(ClientSettings clientSettings, ServiceClientContainer serviceClientContainer)
		{
			if (clientSettings == null)
				throw new ArgumentNullException("settings");
			_clientSettings = clientSettings;
			_serviceClientContainer = serviceClientContainer;
		}

		public ServiceClient GetClient(string name)
		{
			if (!_clientSettings.Clients.ContainsKey(name))
				return null;

			var responses = _clientSettings.Clients[name];
			responses.Sort((left, right) => left.CallCount - right.CallCount);
			var result = responses.First();
			result.CallCount++;
			var data = responses.First();
			var restClient = Map(data);
			return restClient;

		}


		public void AddRestClients(List<ServiceClient> restClients)
		{
			_serviceClientContainer.AddRestClients(restClients);
		}

		public void RemoveRestClients(List<ServiceClient> restClients)
		{
			_serviceClientContainer.RemoveRestClients(restClients);
		}

		public ServiceClient GetRestClient(string name)
		{
			return _serviceClientContainer.GetRestClient(name);
		}

		public List<ServiceClient> GetAllRestClient(string name)
		{
			if (!_clientSettings.Clients.ContainsKey(name))
				return null;

			var responses = _clientSettings.Clients[name];
			var result = new List<ServiceClient>(responses.Count);
			foreach (var data in responses)
			{
				var restClient = Map(data);
				result.Add(restClient);

			}
			return result;
		}

		public List<ServiceClient> GetRestClients()
		{
			var responses = _clientSettings.Clients;
			var result = new List<ServiceClient>(responses.Count);
			foreach (var list in responses.Values)
			{
				foreach (var data in list)
				{
					var restClient = Map(data);
					result.Add(restClient);
				}
			}

			return result;
		}

		private static ServiceClient Map(CommonClientResponse data)
		{
			var restClient = new ServiceClient()
			{
				CallCount = data.CallCount,
				Host = data.Host,
				Name = data.Id,
				Address = data.Address,
				Port = data.Port
			};
			return restClient;
		}
	}
}
