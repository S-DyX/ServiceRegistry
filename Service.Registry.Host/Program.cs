using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
							 // Docker: ServiceRegistry__Host=http://+:7777
							 var urls = jsonConfigurationRoot["ServiceRegistry:Host"];
							 if (!string.IsNullOrWhiteSpace(urls))
								 webBuilder.UseUrls(urls);
							 webBuilder.UseStartup<Startup>();
						 });
		}
	}

}
