using Microsoft.Extensions.Configuration;
using Service.Registry.Configurations;
using Service.Registry.Interfaces.Ver001;
using System.Collections.Generic;
using Service.Registry.Common.Entities;


namespace Service.Registry.Impl.Settings
{
	public sealed class ClientSettings : SettingsBase
	{
		protected override string SectionName => "clients";
		private readonly IConfigurationSection _raw;
		private readonly object _sync = new object();
		private Dictionary<string, List<CommonClientResponse>> _clients;

		public ClientSettings()
		{
			_raw = GetSection();

		}

		public ClientSettings(IConfigurationSection raw)
		{
			_raw = raw;
		}

		private Dictionary<string, List<CommonClientResponse>> GetSettings(IConfigurationSection raw)
		{
			if (raw != null)
			{
				var rawClients = raw.GetChildren();

				var clients = new Dictionary<string, List<CommonClientResponse>>();
				foreach (var client in rawClients)
				{
					Init(client, clients);
				}
				return clients;
			}

			return new Dictionary<string, List<CommonClientResponse>>(0);
		}

		public void Init(IConfigurationSection client, Dictionary<string, List<CommonClientResponse>> clients)
		{
			var attributeId = client.GetSection("id");
			var host = client.GetSection("host");
			var data = client.GetSection("data");
			var address = client.GetSection("address");
			var port = client.GetSection("port");
			var response = new CommonClientResponse()
			{
				Id = attributeId.Value,
				Host = host?.Value,
				Data = data?.Value,
				Address = address?.Value
			};
			if (!string.IsNullOrEmpty(port?.Value))
			{
				response.Port = int.Parse(port.Value);

			}

			if (clients.ContainsKey(response.Id))
			{
				clients[response.Id].Add(response);
			}
			else
			{
				clients.Add(response.Id, new List<CommonClientResponse>() { response });
			}
		}

		public Dictionary<string, List<CommonClientResponse>> Clients
		{
			get
			{
				if (_clients == null)
				{
					lock (_sync)
					{
						if (_clients == null)
							_clients = GetSettings(_raw);
					}
				}

				return _clients;

			}
		}
	}
}
