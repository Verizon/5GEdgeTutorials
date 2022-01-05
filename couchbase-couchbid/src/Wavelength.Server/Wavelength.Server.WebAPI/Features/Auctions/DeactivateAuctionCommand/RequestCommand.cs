using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Wavelength.Server.WebAPI.Features.Auctions.DeactivateAuctionCommand
{
    public class RequestCommand
        : IRequest<bool>
    { 
        //AuctionId 
        [FromRoute]
        public Guid Id { get; set; }

        [FromBody]
        public string AuthCode { get; set; } = string.Empty;
    }
}
