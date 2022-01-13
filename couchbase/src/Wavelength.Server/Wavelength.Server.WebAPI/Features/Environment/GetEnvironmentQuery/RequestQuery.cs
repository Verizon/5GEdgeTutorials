using MediatR;
using Wavelength.Core.DataAccessObjects;

namespace Wavelength.Server.WebAPI.Features.Environment.GetEnvironmentQuery
{
    public class RequestQuery
        : IRequest<AppEnvironmentDAO>
    {
    }
}
