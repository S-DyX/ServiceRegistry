using Service.Registry.Common.Entities;
using Service.Registry.Impl.Settings;
using Service.Registry.Interfaces.Ver001;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Registry.Impl.Services.Ver001
{
	/// <summary>
	/// Clients from the configuration (appsettings.json "Clients") and clients added by Register
	/// are stored in one <see cref="ServiceClientContainer"/>
	/// </summary>
	public sealed class ServiceRegistryClient : IServiceRegistry
	{
		private readonly ServiceClientContainer _serviceClientContainer;

		public ServiceRegistryClient(ClientSettings clientSettings, ServiceClientContainer serviceClientContainer)
		{
			if (clientSettings == null)
				throw new ArgumentNullException(nameof(clientSettings));
			_serviceClientContainer = serviceClientContainer ?? throw new ArgumentNullException(nameof(serviceClientContainer));

			var configured = clientSettings.Clients.Values
				.SelectMany(x => x)
				.Select(Map)
				.ToList();
			_serviceClientContainer.AddRestClients(configured);
		}

		public ServiceClient GetClient(string name)
		{
			return _serviceClientContainer.GetRestClient(name);
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

		private static ServiceClient Map(CommonClientResponse data)
		{
			return new ServiceClient()
			{
				Host = data.Host,
				Name = data.Id,
				Address = data.Address,
				Port = data.Port
			};
		}
	}
}
