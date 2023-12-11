using System.Collections.Generic;
using System.ServiceModel;
using Service.Registry.Common;
using Service.Registry.Common.Entities;
using Service.Registry.Interfaces.Ver001;

namespace Service.Registry.Impl.Services.Ver001
{
	/// <summary>
	/// <see cref="IServiceRegistry"/>
	/// </summary>
	public sealed class ServiceRegistry : IServiceRegistry
	{
		private readonly IServiceRegistry _serviceRegistryClient;

		/// <summary>
		/// Ctor
		/// </summary>
		private ServiceRegistry()
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="serviceRegistryClient"><see cref="IServiceRegistry"/></param>
		public ServiceRegistry(IServiceRegistry serviceRegistryClient)
		{
			_serviceRegistryClient = serviceRegistryClient;
		}


		/// <summary>
		/// <see cref="IServiceRegistry.GetClient(string)"/>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ServiceClient GetClient(string name)
		{
			return _serviceRegistryClient.GetClient(name);
		}


		public void AddRestClients(List<ServiceClient> restClients)
		{
			_serviceRegistryClient.AddRestClients(restClients);
		}

		public void RemoveRestClients(List<ServiceClient> restClients)
		{
			_serviceRegistryClient.RemoveRestClients(restClients);
		}

		/// <summary>
		/// <see cref="IServiceRegistry.GetRestClient(string)"/>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		ServiceClient IServiceRegistry.GetRestClient(string name)
		{
			return _serviceRegistryClient.GetRestClient(name);
		}

		public List<ServiceClient> GetAllRestClient(string name)
		{
			return _serviceRegistryClient.GetAllRestClient(name);
		}

		/// <summary>
		/// <see cref="IServiceRegistry.GetRestClients()"/>
		/// </summary>
		/// <returns></returns>
		List<ServiceClient> IServiceRegistry.GetRestClients()
		{
			return _serviceRegistryClient.GetRestClients();
		}
	}

	
}
