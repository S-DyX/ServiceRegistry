using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Registry.Common;
using Service.Registry.Common.Entities;
using Service.Registry.Impl.Services.Ver001;
using Service.Registry.Impl.Settings;
using Service.Registry.Interfaces.Ver001;

namespace Service.Registry.Host
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder
				.RegisterType<ClientSettings>()
				.As<ClientSettings>()
				.SingleInstance();

			builder
				.RegisterType<ServiceClientContainer>()
				.As<ServiceClientContainer>()
				.SingleInstance();
			builder
				.RegisterType<ServiceRegistryClient>()
				.As<IServiceRegistry>()
				.SingleInstance();

			builder
				.RegisterType<ServiceRegistryClient>()
				.As<IServiceRegistry>()
				.SingleInstance();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseMiddleware(typeof(RequestLoggingMiddleware));
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
