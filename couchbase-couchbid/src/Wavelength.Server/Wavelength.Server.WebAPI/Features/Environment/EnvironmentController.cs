using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Wavelength.Server.WebAPI.Features.Environment
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EnvironmentController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

		[HttpGet]
		public async Task<IActionResult> Get( 
			[FromQuery] GetEnvironmentQuery.RequestQuery requestQuery)
		{
			var response = await _mediator.Send(requestQuery);
			if (response is not null)
			{
				return this.Ok(response);
			}
			return NotFound();
		}
	}
}
