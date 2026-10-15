using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Service.Registry.Host
{
	/// <summary>
	/// Logs requests at Debug level (method, path, status, duration). Request body is not buffered.
	/// </summary>
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger _logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			_next = next;
			_logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
		}

		public async Task Invoke(HttpContext context)
		{
			if (!_logger.IsEnabled(LogLevel.Debug))
			{
				await _next(context);
				return;
			}

			var stopwatch = Stopwatch.StartNew();
			try
			{
				await _next(context);
			}
			finally
			{
				stopwatch.Stop();
				var request = context.Request;
				_logger.LogDebug("{Protocol} {Method} {Path}{Query} => {StatusCode} in {Elapsed} ms",
					request.Protocol, request.Method, request.Path.Value, request.QueryString.Value,
					context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
			}
		}
	}
}
