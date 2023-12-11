using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace Service.Registry.Host
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var jsonConfigurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
															  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
															  .AddEnvironmentVariables().Build();
			return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
						 .UseServiceProviderFactory(new AutofacServiceProviderFactory())
						 .ConfigureWebHostDefaults(webBuilder =>
						 {
							 webBuilder.UseUrls(jsonConfigurationRoot.GetSection("ServiceRegistry:Host").Value)
							 .UseStartup<Startup>();
						 });
		}
	}

}
