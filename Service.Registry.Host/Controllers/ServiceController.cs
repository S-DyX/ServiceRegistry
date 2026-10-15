using Microsoft.AspNetCore.Mvc;
using Service.Registry.Common;
using Service.Registry.Common.Entities;
using Service.Registry.Interfaces.Ver001;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Registry.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ServiceController : ControllerBase
	{
		private readonly IServiceRegistry _serviceRegistry;
		private readonly Lazy<ServiceRegistryFactory> _registryFactory;

		public ServiceController(IServiceRegistry serviceRegistry, Lazy<ServiceRegistryFactory> registryFactory)
		{
			_serviceRegistry = serviceRegistry;
			_registryFactory = registryFactory;
		}

		[HttpGet]
		[Route("Get")]
		public IActionResult Get(string name)
		{
			var client = _serviceRegistry.GetClient(name);
			return Ok(client);
		}

		[HttpGet]
		[Route("all")]
		public IActionResult GetAll(string name = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var clients = _serviceRegistry.GetRestClients();
				return Ok(clients);
			}

			var client = _serviceRegistry.GetAllRestClient(name);
			return Ok(client);
		}


		[HttpGet]
		[Route("Test")]
		public IActionResult Test(string name)
		{
			var client = _registryFactory.Value.GetClientByFactory(name);
			return Ok(client);
		}

		[HttpPost]
		[Route("Register")]
		public IActionResult Register([FromBody] List<ServiceClient> clients)
		{
			var error = Validate(clients, requireEndpoint: true);
			if (error != null)
				return BadRequest(error);
			_serviceRegistry.AddRestClients(clients);
			return Ok();
		}

		[HttpPost]
		[Route("Remove")]
		public IActionResult Remove([FromBody] List<ServiceClient> clients)
		{
			var error = Validate(clients, requireEndpoint: false);
			if (error != null)
				return BadRequest(error);
			_serviceRegistry.RemoveRestClients(clients);
			return Ok();
		}

		private static string Validate(List<ServiceClient> clients, bool requireEndpoint)
		{
			if (clients == null || clients.Count == 0)
				return "empty clients";
			if (clients.Any(x => x == null || string.IsNullOrWhiteSpace(x.Name)))
				return "client name is required";
			if (requireEndpoint && clients.Any(x => string.IsNullOrWhiteSpace(x.Host) && string.IsNullOrWhiteSpace(x.Address)))
				return "client host or address is required";
			return null;
		}
	}
}
