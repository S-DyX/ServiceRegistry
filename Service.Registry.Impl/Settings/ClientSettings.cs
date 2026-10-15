using Microsoft.Extensions.Configuration;
using Service.Registry.Configurations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Service.Registry.Common.Entities;


namespace Service.Registry.Impl.Settings
{
	public sealed class ClientSettings : SettingsBase
	{
		protected override string SectionName => "clients";
		private readonly IConfigurationSection _raw;
		private readonly object _sync = new object();
		private Dictionary<string, List<CommonClientResponse>> _clients;

		/// <summary>
		/// Ctor, section "Clients" is optional
		/// </summary>
		public ClientSettings()
		{
			_raw = GetElement(true);
		}

		public ClientSettings(IConfigurationSection raw)
		{
			_raw = raw;
		}

		private Dictionary<string, List<CommonClientResponse>> GetSettings(IConfigurationSection raw)
		{
			var clients = new Dictionary<string, List<CommonClientResponse>>(StringComparer.Ordinal);
			if (raw == null)
				return clients;

			foreach (var client in raw.GetChildren())
			{
				Init(client, clients);
			}
			return clients;
		}

		public void Init(IConfigurationSection client, Dictionary<string, List<CommonClientResponse>> clients)
		{
			var id = client["id"]?.Trim();
			if (string.IsNullOrEmpty(id))
				throw new InvalidDataException($"Configuration \"{client.Path}\": \"id\" is required.");

			var response = new CommonClientResponse()
			{
				Id = id,
				Host = client["host"],
				Data = client["data"],
				Address = client["address"]
			};

			var port = client["port"];
			if (!string.IsNullOrWhiteSpace(port))
			{
				if (!int.TryParse(port, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value <= 0 || value > 65535)
					throw new InvalidDataException($"Configuration \"{client.Path}\" ({id}): invalid port \"{port}\".");
				response.Port = value;
			}

			if (clients.TryGetValue(response.Id, out var list))
			{
				list.Add(response);
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
