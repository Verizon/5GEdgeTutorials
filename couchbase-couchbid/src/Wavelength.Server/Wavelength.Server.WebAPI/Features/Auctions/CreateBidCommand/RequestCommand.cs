using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using Wavelength.Core.DataAccessObjects;

namespace Wavelength.Server.WebAPI.Features.Auction.CreateBidCommand
{
    public class RequestCommand
        : IRequest<BidDAO> 
    {
        //AuctionId - required to help calculate a winner
        [FromRoute]
        public Guid Id { get; set; }  

        //DeviceId  - unique Id of the App running on a mobild device
        [FromBody]
        public Guid DeviceId { get; set; } 

        //BidId - unique to make a bid, but can have bids from different locations
        [FromBody]
        public Guid BidId { get; set; }  
        
        //Sent - when request was sent from mobile client
        [FromBody]
        public DateTimeOffset Sent { get; set; }

        public DateTimeOffset Received { get; private set; } = DateTimeOffset.UtcNow;
    }
}
