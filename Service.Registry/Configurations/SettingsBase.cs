using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;

namespace Service.Registry.Configurations
{
	/// <summary>
	/// Абстрактный базовый класс настроек
	/// </summary>
	public abstract class SettingsBase
	{
		private const string FileName = "appsettings.json";

		private static readonly Lazy<IConfigurationRoot> Configuration =
			new Lazy<IConfigurationRoot>(Load, LazyThreadSafetyMode.ExecutionAndPublication);

		protected static IConfigurationRoot ConfigurationRoot => Configuration.Value;

		private static IConfigurationRoot Load()
		{
			var directory = Directory.GetCurrentDirectory();
			try
			{
				return new ConfigurationBuilder()
					.SetBasePath(directory)
					.AddJsonFile(FileName, optional: true, reloadOnChange: false)
					.Build();
			}
			catch (Exception e)
			{
				throw new InvalidDataException($"Failed to load configuration file {Path.Combine(directory, FileName)}: {e.Message}", e);
			}
		}

		/// <summary>
		/// Имя конфигурационной секции
		/// </summary>
		protected abstract string SectionName { get; }

		/// <summary>
		/// Получаем обязательную конфигурационную секцию
		/// </summary>
		/// <returns></returns>
		protected IConfigurationSection GetSection()
		{
			return GetElement(false);
		}

		/// <summary>
		/// Получаем конфигурационную секцию
		/// </summary>
		/// <param name="ignore">true - вернуть null, если секции нет</param>
		/// <returns></returns>
		protected IConfigurationSection GetElement(bool ignore)
		{
			var section = ConfigurationRoot.GetSection(SectionName);
			if (section.Exists())
				return section;

			if (ignore)
				return null;
			throw new InvalidDataException($"Required section \"{SectionName}\" not found in {FileName}.");
		}
	}
}
