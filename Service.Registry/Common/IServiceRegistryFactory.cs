using System.Collections.Generic;
using Service.Registry.Common.Entities;

namespace Service.Registry.Common
{
	/// <summary>
	/// Service registry factory
	/// </summary>
	public interface IServiceRegistryFactory
	{
		/// <summary>
		/// Method return configuration string
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string GetCommonClient(string name);

		/// <summary>
		/// Method create rest clinet by name
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		T CreateRest<T>(string name = null) where T : class;

		/// <summary>
		/// This is for except type load error in IIS
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		T CreateRest<T, TInstance>(string name = null) where T : class;

		/// <summary>
		/// This is for except type load error in IIS
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TInstance"></typeparam>
		/// <returns></returns>
		List<T> CreateAllRest<T, TInstance>(string name = null) where T : class;

		/// <summary>
		/// Add new rest client
		/// </summary>
		/// <param name="clients"></param>
		void AddClients(List<ServiceClient> clients);
		void RemoveClients(List<ServiceClient> clients);

		/// <summary>
		/// <see cref="ServiceClient"/>
		/// </summary>
		/// <returns></returns>
		List<ServiceClient> GetRestClients();

		/// <summary>
		/// Create tcp client concrete type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TInstance"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		T CreateTcp<T, TInstance>(string name = null) where T : class;

		/// <summary>
		/// Create tcp client
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		T CreateTcp<T>(string name = null) where T : class;
	}
}
