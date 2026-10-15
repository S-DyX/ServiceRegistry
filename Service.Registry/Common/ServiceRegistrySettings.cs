using Service.Registry.Configurations;
using System.IO;

namespace Service.Registry.Common
{
	public sealed class ServiceRegistrySettings : SettingsBase
	{
		protected override string SectionName => "serviceRegistry";


		public ServiceRegistrySettings()
		{
			const string sample = "\"serviceRegistry\": { \"address\": \"http://localhost:7777\" }";

			var raw = GetElement(true);
			Address = raw?["address"]?.Trim();
			if (string.IsNullOrEmpty(Address))
				throw new InvalidDataException($"Please add section {sample} to file appsettings.json");

			Address = Address.TrimEnd('/');
		}
		public string Address { get; private set; }
	}
}
