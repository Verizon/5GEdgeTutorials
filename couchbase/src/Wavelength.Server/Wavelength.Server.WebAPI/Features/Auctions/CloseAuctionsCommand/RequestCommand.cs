using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Wavelength.Server.WebAPI.Features.Auctions.CloseAuctionsCommand
{
    public class RequestCommand
        : IRequest<IEnumerable<string>>
    {
        [FromBody]
        public string ClosingCode { get; set; } = string.Empty;
    }
}
