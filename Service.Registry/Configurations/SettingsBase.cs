using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Service.Registry.Configurations
{
	/// <summary>
	/// Абстрактный базовый класс настроек
	/// </summary>
	public abstract class SettingsBase
	{
		private static object _sync = new object();
		private static IConfigurationRoot _configuration;
		protected static IConfigurationRoot ConfigurationRoot
		{
			get
			{
				if (_configuration == null)
				{
					lock (_sync)
						if (_configuration == null)
						{
							//to do надо переделать
							TryLoad();
						}
				}
				return _configuration;
			}
		}

		private static void TryLoad()
		{
			try
			{
				var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

				_configuration = builder.Build();
			}
			catch { }
		}

		/// <summary>
		/// Имя конфигурационной секции
		/// </summary>
		protected abstract string SectionName { get; }

		/// <summary>
		/// Получаем элемент конфигурационной секции
		/// </summary>
		/// <returns></returns>
		protected IConfigurationSection GetSection()
		{
			var section = ConfigurationRoot.GetSection(SectionName);

			if (section == null)
				throw new InvalidDataException($"Required section \"{SectionName}\" not found.");

			return section;
		}
		protected IConfigurationSection GetElement(bool ignore)
		{
			var section = ConfigurationRoot.GetSection(SectionName);

			if (section == null)
			{
				if (ignore)
					return null;
				throw new InvalidDataException($"Required section \"{SectionName}\" not found.");
			}

			return section;
		}


	}
}
