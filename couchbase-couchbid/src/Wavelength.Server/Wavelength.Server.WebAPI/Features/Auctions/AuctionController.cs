using Couchbase.Core.Exceptions.KeyValue;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Wavelength.Server.WebAPI.Core.Exceptions;

namespace Wavelength.Server.WebAPI.Features.Auctions
{
    [ApiController]
	[Route("/api/v1/[controller]")]
	public class AuctionController : ControllerBase
	{
		private readonly ILogger<AuctionController> _logger;
		private readonly IMediator _mediator;
		private readonly IWebHostEnvironment _hostEnvironment;
		private const string ServerTimingHeaderName = "Server-Timing";

		public AuctionController(
			ILogger<AuctionController> logger,
			IMediator medator,
			IWebHostEnvironment hostEnvironment)
		{
			_logger = logger;
			_mediator = medator;
			_hostEnvironment = hostEnvironment;
		}

		[HttpGet]
		public async Task<IActionResult> Get(
			[FromQuery] GetAuctionsQuery.RequestQuery requestQuery)
		{
			var stopWatch = new Stopwatch();
			try
			{
				stopWatch.Start();
				var response = await _mediator.Send(requestQuery);
				if (response is not null)
				{
					stopWatch.Stop();
					response.PerformanceMetrics.ApiLatency = stopWatch.Elapsed.TotalMilliseconds;
					this.HttpContext.Response.Headers[ServerTimingHeaderName] = string.Join(",", response.PerformanceMetrics.ToHeaders());
					return this.Ok(response);
				}
				stopWatch.Stop();
				return NotFound();
			} 
			catch (Exception ex) 
			{
				stopWatch.Stop();
				return DealWithErrors(ex);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post(
			[FromBody] CreateAuctionCommand.RequestCommand command)
        {
            try
            {
				var response = await _mediator.Send(command);
				if (response is not null)
                {
					return Created($"/api/v1/Auction/{response.Id}", response);
                }
				return Problem("Couldn't create item");
            }
            catch (Exception ex)
            {
				return DealWithErrors(ex);
            }
        }

		[HttpPut]
		[Route("[action]")]
		public async Task<IActionResult> Deactivate(
		  [FromBody] DeactivateAuctionCommand.RequestCommand requestCommand)
		{
			try
			{
				var response = await _mediator.Send(requestCommand);
				if (response) 
				{
					return this.Ok(response);
				}
				return this.Problem();
			}
			catch (Exception ex)
			{
				return DealWithErrors(ex);
			}
		}

		[HttpPut]
		[Route("[action]")]
		public async Task<IActionResult> Close(
				  [FromBody] CloseAuctionsCommand.RequestCommand requestCommand)
		{
            try
            {
				var response = await _mediator.Send(requestCommand);
				if (response is not null)
				{
					return this.Ok(response);
				}
				return this.Problem();
			}
            catch (Exception ex)
            {
				return DealWithErrors(ex);
            }
		}

		[HttpDelete]
		public async Task<IActionResult> Delete(
			[FromBody] DeleteAuctionCommand.RequestCommand requestCommand)
		{
			try
			{
				var response = await _mediator.Send(requestCommand);
				if (response)
				{
					return this.Ok(response);
				}
				return this.Problem();
			}
			catch (Exception ex)
			{
				return DealWithErrors(ex);
			}
		}

		[HttpPost]
		[Route("[action]")]
		public async Task<IActionResult> Bid(
			[FromBody] Auction.CreateBidCommand.RequestCommand requestCommand)
		{
			var stopWatch = new Stopwatch();
			try
			{
				stopWatch.Start();
				var response = await _mediator.Send(requestCommand);
				if (response is not null)
				{
					stopWatch.Stop();
					response.PerformanceMetrics.ApiLatency = stopWatch.Elapsed.TotalMilliseconds;
					response.PerformanceMetrics.ApiSendDateTime = DateTimeOffset.UtcNow;
					this.HttpContext.Response.Headers[ServerTimingHeaderName] = string.Join(",", response.PerformanceMetrics.ToHeaders());
					return this.Ok(response);
				}
				stopWatch.Stop();
				return this.Problem();
			}
			catch (AuctionEndedException)
			{
				return this.ValidationProblem();
			}
			catch (Exception ex)
				when (ex is DocumentNotFoundException || ex is AuctionNotFoundException)
			{
				stopWatch.Stop();
				return this.NotFound();
			}
			catch (Exception ex)
			{
				stopWatch.Stop();
				return DealWithErrors(ex);
			}
		}


		private IActionResult DealWithErrors(Exception ex) 
		{
			Console.WriteLine($"Error: {ex.Message} StackTrace: {ex.StackTrace}");
			switch (_hostEnvironment.EnvironmentName)
			{
				case Constants.Environments.Development:
				case Constants.Environments.Staging:
				case Constants.Environments.UAT:
					return this.Problem(ex.StackTrace, null, null, ex.Message, null);
				default:
					return this.Problem();

			}
		}
	}
}
