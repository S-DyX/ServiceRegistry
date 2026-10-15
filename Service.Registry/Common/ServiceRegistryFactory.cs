using Newtonsoft.Json;
using Service.Registry.Common.Entities;
using Service.Registry.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Service.Registry.Common
{

	/// <summary>
	/// <see cref="IServiceRegistryFactory"/>
	/// </summary>
	public sealed class ServiceRegistryFactory : IServiceRegistryFactory
	{
		/// <summary>
		/// Timeout of requests to the service registry
		/// </summary>
		private static readonly TimeSpan RegistryTimeout = TimeSpan.FromSeconds(5);
		private static readonly Type BaseType = typeof(RestClientResponseBase);
		private static readonly Type TcpBaseType = typeof(TcpClientResponseBase);
		private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();
		private static volatile bool _isLoad;
		private static readonly object Sync = new object();

		private readonly ServiceRegistrySettings _settings;
		private readonly IHttpClientFactory _clientFactory;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="clientFactory">if null <see cref="InternalHttpClientFactory"/> is used</param>
		public ServiceRegistryFactory(IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory ?? InternalHttpClientFactory.Default;
			_settings = new ServiceRegistrySettings();
		}

		/// <summary>
		/// Used <see cref="InternalHttpClientFactory"/>
		/// </summary>
		public ServiceRegistryFactory()
			: this(null)
		{
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
			var foundType = GetImplementationType<T>(BaseType);

			if (string.IsNullOrWhiteSpace(name))
				name = typeof(T).Name;

			var clients = GetClientsByFactory(name);
			if (clients == null || clients.Count == 0)
				return new List<T>();

			var result = new List<T>(clients.Count);
			foreach (var client in clients)
			{
				if (string.IsNullOrEmpty(client?.Host))
					continue;
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
			var type = typeof(T);
			var foundType = GetImplementationType<T>(BaseType);

			var serviceClient = GetServiceClient(name, type.Name, type.FullName);
			if (string.IsNullOrEmpty(serviceClient?.Host))
				return null;

			return GetRestInstance<T>(serviceClient, foundType);
		}

		private T GetRestInstance<T>(ServiceClient serviceClient, Type foundType) where T : class
		{
			var settings = new RestClientResponseSettings()
			{
				Host = serviceClient.Host
			};

			var withFactory = foundType.GetConstructor(new[] { typeof(RestClientResponseSettings), typeof(IHttpClientFactory) });
			var instance = withFactory != null
				? Activator.CreateInstance(foundType, settings, _clientFactory)
				: Activator.CreateInstance(foundType, settings);

			return (T)instance;
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
			Post("Register", clients);
		}

		/// <summary>
		/// Remove client <see cref="ServiceClient"/> from server
		/// </summary>
		/// <param name="clients"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void RemoveClients(List<ServiceClient> clients)
		{
			Post("Remove", clients);
		}

		public List<ServiceClient> GetRestClients()
		{
			return GetClientsByFactory() ?? new List<ServiceClient>();
		}

		public T CreateTcp<T, TInstance>(string name = null) where T : class
		{
			return CreateTcp<T>(name);
		}
		public T CreateTcp<T>(string name = null) where T : class
		{
			var type = typeof(T);
			var foundType = GetImplementationType<T>(TcpBaseType);

			var host = GetServiceClient(name, type.Name, type.FullName);
			if (host == null)
				return null;

			var settings = new TcpClientResponseSettings()
			{
				Address = host.Address,
				Port = host.Port
			};

			return (T)Activator.CreateInstance(foundType, settings);
		}

		/// <summary>
		/// Returns the first client found by names (empty and duplicate names are skipped)
		/// </summary>
		private ServiceClient GetServiceClient(params string[] names)
		{
			foreach (var name in names.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.Ordinal))
			{
				var client = GetClientByFactory(name);
				if (client != null)
					return client;
			}

			return null;
		}

		public ServiceClient GetClientByFactory(string name)
		{
			var url = $"{_settings.Address}/api/service/get?name={Uri.EscapeDataString(name ?? string.Empty)}";
			var res = GetString(url);
			if (string.IsNullOrWhiteSpace(res))
				return null;
			return JsonConvert.DeserializeObject<ServiceClient>(res);
		}

		private List<ServiceClient> GetClientsByFactory(string name = null)
		{
			var url = string.IsNullOrEmpty(name)
				? $"{_settings.Address}/api/service/all"
				: $"{_settings.Address}/api/service/all?name={Uri.EscapeDataString(name)}";
			var res = GetString(url);
			if (string.IsNullOrWhiteSpace(res))
				return null;
			return JsonConvert.DeserializeObject<List<ServiceClient>>(res);
		}

		private string GetString(string url)
		{
			try
			{
				return RunSyncUtil.RunSync(async () =>
				{
					using var cts = new CancellationTokenSource(RegistryTimeout);
					using var response = await _clientFactory.CreateClient().GetAsync(url, cts.Token).ConfigureAwait(false);
					response.EnsureSuccessStatusCode();
					var bytes = await response.Content.ReadAsByteArrayAsync(cts.Token).ConfigureAwait(false);
					return Encoding.UTF8.GetString(bytes);
				});
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"GET {url} failed: {e.Message}", e);
			}
		}

		private void Post(string method, List<ServiceClient> clients)
		{
			if (clients == null)
				throw new ArgumentNullException(nameof(clients));

			var url = $"{_settings.Address}/api/service/{method}";
			try
			{
				var json = JsonConvert.SerializeObject(clients);
				RunSyncUtil.RunSync(async () =>
				{
					using var cts = new CancellationTokenSource(RegistryTimeout);
					using var content = new StringContent(json, Encoding.UTF8, "application/json");
					using var response = await _clientFactory.CreateClient().PostAsync(url, content, cts.Token).ConfigureAwait(false);
					response.EnsureSuccessStatusCode();
				});
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"POST {url} failed: {e.Message}", e);
			}
		}

		private static Type GetImplementationType<T>(Type baseType) where T : class
		{
			var type = typeof(T);
			var key = $"{baseType.FullName}|{type.AssemblyQualifiedName}";
			return TypeCache.GetOrAdd(key, _ => ResolveImplementationType(type, baseType));
		}

		/// <summary>
		/// Deterministic choice of implementation: not obsolete, then name equals interface name without "I", then by full name
		/// </summary>
		private static Type ResolveImplementationType(Type type, Type baseType)
		{
			var candidates = FindTypes(type)
				.Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && baseType.IsAssignableFrom(t))
				.Distinct()
				.ToList();

			if (candidates.Count == 0)
			{
				throw new InvalidOperationException(
					$"Not found class implementing {type.FullName} and delivered from {baseType.FullName} in {AppContext.BaseDirectory}. Please load assembly with delivered class");
			}

			var expectedName = type.IsInterface && type.Name.Length > 1 && type.Name[0] == 'I'
				? type.Name.Substring(1)
				: type.Name;

			return candidates
				.OrderBy(t => t.IsDefined(typeof(ObsoleteAttribute), false) ? 1 : 0)
				.ThenBy(t => string.Equals(t.Name, expectedName, StringComparison.Ordinal) ? 0 : 1)
				.ThenBy(t => t.FullName, StringComparer.Ordinal)
				.First();
		}

		private static List<Type> FindTypes(Type type)
		{
			var result = GetTypes(type, AppDomain.CurrentDomain.GetAssemblies());
			if (result.Count > 0 || _isLoad)
				return result;

			lock (Sync)
			{
				if (!_isLoad)
				{
					LoadAssemblies();
					_isLoad = true;
				}
			}

			return GetTypes(type, AppDomain.CurrentDomain.GetAssemblies());
		}

		private static void LoadAssemblies()
		{
			var path = Path.GetDirectoryName(typeof(ServiceRegistryFactory).Assembly.Location);
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				path = AppContext.BaseDirectory;

			var loaded = new HashSet<string>(
				AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name),
				StringComparer.OrdinalIgnoreCase);

			foreach (var dll in Directory.GetFiles(path, "*.dll"))
			{
				try
				{
					var assemblyName = AssemblyName.GetAssemblyName(dll);
					if (!loaded.Add(assemblyName.Name))
						continue;
					// LoadFrom loads into the default context, so types are not duplicated
					Assembly.LoadFrom(dll);
				}
				catch (Exception e) when (e is FileLoadException || e is BadImageFormatException || e is FileNotFoundException)
				{
				}
			}
		}

		private static List<Type> GetTypes(Type type, IEnumerable<Assembly> assemblies)
		{
			var result = new List<Type>();
			foreach (var assembly in assemblies)
			{
				if (assembly.IsDynamic)
					continue;

				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					types = e.Types.Where(t => t != null).ToArray();
				}
				catch (Exception)
				{
					continue;
				}

				foreach (var t in types)
				{
					try
					{
						if (type.IsAssignableFrom(t))
							result.Add(t);
					}
					catch (Exception)
					{
						// type with unresolved dependencies
					}
				}
			}

			return result;
		}
	}
}
