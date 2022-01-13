using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using Wavelength.Core.DataAccessObjects;

namespace Wavelength.Server.WebAPI.Features.Auctions.CreateAuctionCommand
{
    public class RequestCommand
        : IRequest<AuctionItemDAO>
    {
        [FromBody]
        public string Title { get; set; } = string.Empty;

        [FromBody]
        public string ImageUrl { get; set; } = string.Empty;

        [FromBody]
        public DateTimeOffset StopTime { get; set; }

        [FromBody]
        public string AuthCode { get; set; } = string.Empty;
    }
}
