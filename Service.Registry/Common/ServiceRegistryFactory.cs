using Newtonsoft.Json;
using Service.Registry.Common.Entities;
using Service.Registry.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Service.Registry.Common
{

	/// <summary>
	/// <see cref="IServiceRegistryFactory"/>
	/// </summary>
	public sealed class ServiceRegistryFactory : IServiceRegistryFactory
	{
		private readonly ServiceRegistrySettings _settings;
		private static Type _baseType = typeof(RestClientResponseBase);
		private static Type _tcpBaseType = typeof(TcpClientResponseBase);
		private static readonly ConcurrentDictionary<string, Type> ConcurrentDictionary = new ConcurrentDictionary<string, Type>();
		private static bool _isLoad;
		private static object _sync = new object();
		private string _host;
		private readonly IHttpClientFactory _clientFactory;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="clientFactory"></param>
		public ServiceRegistryFactory(IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory;
			_settings = new ServiceRegistrySettings();
			_host = _settings.Address;

		}

		/// <summary>
		/// Used <see cref="InternalHttpClientFactory"/>
		/// </summary>
		public ServiceRegistryFactory()
		{
			_clientFactory = new InternalHttpClientFactory();
			_settings = new ServiceRegistrySettings();
			_host = _settings.Address;

		}


		/// <summary>
		/// <see cref="IServiceRegistryFactory.CreateRest{T,TInstance}(string)"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TInstance"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public T CreateRest<T, TInstance>(string name = null) where T : class
		{
			return CreateRest<T>(name);
		}

		/// <summary>
		/// <see cref="IServiceRegistryFactory.CreateAllRest{T,TInstance}(string)"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TInstance"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public List<T> CreateAllRest<T, TInstance>(string name = null) where T : class
		{
			var type = GetType<T>(_baseType, out var foundType);
			if (!_baseType.IsAssignableFrom(foundType))
				throw new InvalidOperationException($"Type {foundType.FullName} is not delivered from {_baseType.FullName}. Or not found delivered class, please load assembly with delivered class");
 
			if (string.IsNullOrWhiteSpace(name))
				name = type.Name;

			var clients = GetClientsByFactory(name);

			var result = new List<T>(clients.Count); 

			foreach (var client in clients)
			{

				result.Add(GetRestInstance<T>(client, foundType));
			}

			return result;
		}

		/// <summary>
		/// <see cref="IServiceRegistryFactory.CreateRest{T}(string)"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public T CreateRest<T>(string name = null) where T : class
		{
			var type = GetType<T>(_baseType, out var foundType);

			if (!_baseType.IsAssignableFrom(foundType))
				throw new InvalidOperationException($"Type {foundType.FullName} is not delivered from {_baseType.FullName}. Or not found delivered class, please load assembly with delivered class");
			ServiceClient serviceClient = null;
			if (name != null)
			{
				serviceClient = GetServiceClient(name, name);
			}
			if (serviceClient == null)
				serviceClient = GetServiceClient(type.FullName, type.Name);
			if (serviceClient?.Host == null)
			{
				return default(T);
			}
			return GetRestInstance<T>(serviceClient, foundType);
		}

		private T GetRestInstance<T>(ServiceClient serviceClient, Type foundType) where T : class
		{
			var settings = new RestClientResponseSettings()
			{
				Host = serviceClient.Host
			};

			var httpClientFactoryParameter = foundType.GetConstructors()
				.Any(x => x.GetParameters()
					.Any(z => z.ParameterType.Name == nameof(IHttpClientFactory)));

			if (httpClientFactoryParameter)
			{
				return Activator.CreateInstance(foundType, settings, _clientFactory) as T;
			}

			return Activator.CreateInstance(foundType, settings) as T;
		}


		public string GetCommonClient(string name)
		{
			return GetClientByFactory(name)?.Host;
		}


		/// <summary>
		/// Add client <see cref="ServiceClient"/> to server
		/// </summary>
		/// <param name="clients"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void AddClients(List<ServiceClient> clients)
		{
			var client = _clientFactory.CreateClient();
			var url = $"{_settings.Address}/api/service/Register";
			try
			{
				var serializeObject = JsonConvert.SerializeObject(clients);
				var stringContent = new StringContent(serializeObject, Encoding.UTF8,
					"application/json");
				client.PostAsync(url, stringContent).Wait(2000);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"{url}", e);
			}
		}

		/// <summary>
		/// Remove client <see cref="ServiceClient"/> from server
		/// </summary>
		/// <param name="clients"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void RemoveClients(List<ServiceClient> clients)
		{
			var client = _clientFactory.CreateClient();
			var url = $"{_settings.Address}/api/service/Remove";
			try
			{
				var serializeObject = JsonConvert.SerializeObject(clients);
				var stringContent = new StringContent(serializeObject, Encoding.UTF8,
					"application/json");
				client.PostAsync(url, stringContent).Wait(2000);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"{url}", e);
			}
		}

		public List<ServiceClient> GetRestClients()
		{
			return GetClientsByFactory();
		}

		public T CreateTcp<T, TInstance>(string name = null) where T : class
		{
			return CreateTcp<T>(name);
		}
		public T CreateTcp<T>(string name = null) where T : class
		{
			var type = GetType<T>(_tcpBaseType, out var foundType);

			if (!_tcpBaseType.IsAssignableFrom(foundType))
				throw new InvalidOperationException($"Type {foundType.FullName} is not delivered from {_tcpBaseType.FullName}. Or not found delivered class, please load assembly with delivered class");

			ServiceClient host = null;
			if (name != null)
			{
				host = GetServiceClient(name, name);
			}
			host ??= GetServiceClient(type.FullName, type.Name);

			if (host == null)
			{
				return default(T);
			}
			var settings = new TcpClientResponseSettings()
			{
				Address = host.Address,
				Port = host.Port
			};


			return Activator.CreateInstance(foundType, settings) as T;
		}

		private List<ServiceClient> GetClients()
		{
			using (var webClient = new WebClient())
			{
				webClient.Encoding = Encoding.UTF8;
				var res = webClient.DownloadString($"{_settings.Address}/api/service/all");
				if (string.IsNullOrEmpty(res))
					return null;
				var restClient = JsonConvert.DeserializeObject<List<ServiceClient>>(res);
				return restClient;
			}
		}

		private static List<Type> FindType<T>()
		{
			var type = typeof(T);
			var assemblies = AppDomain.CurrentDomain
				.GetAssemblies().ToList();
			var result = GetType(type, assemblies);
			if (result.Any())
				return result;
			if (!_isLoad)
			{
				lock (_sync)
				{
					if (!_isLoad)
					{
						string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
						foreach (string dll in Directory.GetFiles(path, "*.dll"))
						{
							try
							{
								assemblies.Add(Assembly.LoadFile(dll));
							}
							catch (FileLoadException)
							{ }
							catch (BadImageFormatException)
							{ }

						}
						_isLoad = true;
					}
				}
			}

			return GetType(type, assemblies);
		}

		private static List<Type> GetType(Type type, List<Assembly> assemblies)
		{
			var result = new List<Type>();
			foreach (var a in assemblies)
			{
				try
				{
					var types = a.GetTypes();
					foreach (var t in types)
					{

						if (type.IsAssignableFrom(t))
						{
							result.Add(t);
						}
					}
				}
				catch (TypeLoadException)
				{
				}
				catch (Exception ex)
				{
					;
				}
			}

			return result;
		}
		private ServiceClient GetServiceClient(string fullName, string name)
		{
			var restClient = GetClientByFactory(name) ?? GetClientByFactory(fullName);
			if (restClient != null)
			{
				return restClient;
			}

			var response = GetClientByFactory(name) ?? GetClientByFactory(fullName);
			if (response == null)
				return null;


			return response;
		}
		public ServiceClient GetClientByFactory(string name)
		{
			var client = _clientFactory.CreateClient();
			var url = $"{_settings.Address}/api/service/get?name={name}";
			try
			{
				var bytes = RunSyncUtil.RunSync<byte[]>(() => client.GetByteArrayAsync(url));
				var res = Encoding.UTF8.GetString(bytes);
				if (string.IsNullOrEmpty(res))
					return null;
				var restClient = JsonConvert.DeserializeObject<ServiceClient>(res);
				return restClient;
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"{url}", e);
			}
		}

		private List<ServiceClient> GetClientsByFactory(string name = null)
		{
			var client = _clientFactory.CreateClient();
			//client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8", 0.5));
			client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

			var bytes = RunSyncUtil.RunSync<byte[]>(() =>
				client.GetByteArrayAsync($"{_settings.Address}/api/service/all?name={name}"));
			var res = Encoding.UTF8.GetString(bytes);
			if (string.IsNullOrEmpty(res))
				return null;
			var restClients = JsonConvert.DeserializeObject<List<ServiceClient>>(res);
			return restClients;
		}


		private static Type GetType<T>(Type baseType, out Type foundType) where T : class
		{
			var type = typeof(T);
			foundType = type;
			var key = $"{baseType.Name}_{foundType.FullName}";
			if (ConcurrentDictionary.ContainsKey(key))
			{
				foundType = ConcurrentDictionary[key];
			}
			else
			{
				var types = FindType<T>();
				var location = Assembly.GetExecutingAssembly().Location;
				//var location = Directory.GetCurrentDirectory();
				if (!types.Any())
				{
					throw new InvalidOperationException(
						$"Not found Any Delivered types {type.FullName} in assemblies {location}");
				}

				foundType = types.FirstOrDefault(t => baseType.IsAssignableFrom(t));
				if (foundType == null)
					throw new InvalidOperationException(
						$"Delivered type not found for {type.FullName}, {baseType.FullName},in assemblies {location},{string.Join(",", types.Select(x => x.FullName))}");
				ConcurrentDictionary.TryAdd(key, foundType);
			}

			return type;
		}
	}
}
