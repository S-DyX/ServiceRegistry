using Microsoft.AspNetCore.Mvc;
using Service.Registry.Common;
using Service.Registry.Common.Entities;
using Service.Registry.Interfaces.Ver001;
using System.Collections.Generic;

namespace Service.Registry.Host.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ServiceController : ControllerBase
	{
		private readonly IServiceRegistry _serviceRegistry;

		public ServiceController(IServiceRegistry serviceRegistry)
		{
			this._serviceRegistry = serviceRegistry;
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

				var client = _serviceRegistry.GetRestClients();
				return Ok(client);
			}
			else
			{
				var client = _serviceRegistry.GetAllRestClient(name);
				return Ok(client); 
			}
		}


		[HttpGet]
		[Route("Test")]
		public IActionResult Test(string name)
		{
			var ss = new ServiceRegistryFactory().GetClientByFactory(name);
			return Ok(ss);
		}
		[HttpPost]
		[Route("Register")]
		public IActionResult Register([FromBody] List<ServiceClient> clients)
		{
			if (clients == null)
				return BadRequest("empty clients");
			_serviceRegistry.AddRestClients(clients);
			return Ok();
		}

		[HttpPost]
		[Route("Remove")]
		public IActionResult Remove([FromBody] List<ServiceClient> clients)
		{
			if (clients == null)
				return BadRequest("empty clients");
			_serviceRegistry.RemoveRestClients(clients);
			return Ok();
		}
	}
}
