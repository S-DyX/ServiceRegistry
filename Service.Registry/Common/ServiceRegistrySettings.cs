using Service.Registry.Configurations;
using System.IO;

namespace Service.Registry.Common
{
	public sealed class ServiceRegistrySettings : SettingsBase
	{
		protected override string SectionName => "serviceRegistry";


		public ServiceRegistrySettings()
		{
			var raw = GetSection();
			if (raw == null)
			{
				var s = "\"serviceRegistry\": { \"address\": \"http://localhost:7777\" }";
				throw new InvalidDataException($"Please add section {s} to file appsettings.json");
			}

			Address = raw.GetSection("address")?.Value;
			if (Address!=null && Address.EndsWith("/"))
				Address = Address.Substring(0, Address.Length - 1);
		}
		public string Address { get; private set; }
	}
}
