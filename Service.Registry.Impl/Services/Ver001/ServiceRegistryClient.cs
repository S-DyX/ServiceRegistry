using Service.Registry.Common;
using Service.Registry.Impl.Settings;
using Service.Registry.Interfaces.Ver001;
using System;
using System.Collections.Generic;
using System.Linq;
using Service.Registry.Common.Entities;

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
			var restClient = new ServiceClient()
			{
				CallCount = result.CallCount,
				Host = data.Host,
				Name = data.Id,
				Address = data.Address,
				Port = data.Port
			};
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
			return _serviceClientContainer.GetAllRestClient(name);
		}

		public List<ServiceClient> GetRestClients()
		{
			return _serviceClientContainer.GetRestClients();
		}
	}
}
