using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Service.Registry.Common;
using Service.Registry.Common.Entities;

namespace Service.Registry.Interfaces.Ver001
{
	/// <summary>
	/// Service registry 
	/// </summary>
	public interface IServiceRegistry
	{
		/// <summary>
		/// Method return <see cref="ServiceClient"/> client by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		ServiceClient GetClient(string name);

		/// <summary>
		/// Method add <see cref="ServiceClient"/> clients
		/// </summary>
		/// <param name="restClients"></param>
		void AddRestClients(List<ServiceClient> restClients);


		/// <summary>
		/// Method remove <see cref="ServiceClient"/> clients
		/// </summary>
		/// <param name="restClients"></param>
		void RemoveRestClients(List<ServiceClient> restClients);

		/// <summary>
		/// Method return <see cref="ServiceClient"/> by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		ServiceClient GetRestClient(string name);

		/// <summary>
		/// Method return <see cref="ServiceClient"/> by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		List<ServiceClient> GetAllRestClient(string name);

		/// <summary>
		/// Method return all <see cref="ServiceClient"/>
		/// </summary>
		/// <returns></returns>
		List<ServiceClient> GetRestClients();
	}
}
