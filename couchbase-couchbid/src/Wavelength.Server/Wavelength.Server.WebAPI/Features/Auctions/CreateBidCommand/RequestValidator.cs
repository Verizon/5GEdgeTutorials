using FluentValidation;

namespace Wavelength.Server.WebAPI.Features.Auction.CreateBidCommand
{
    public class RequestValidator
        : AbstractValidator<RequestCommand>
    {
        public RequestValidator()
        {
            //validate auctionId
            RuleFor(r => r.Id)
                .NotEmpty()
                .NotNull();

            RuleFor(r => r.DeviceId)
                .NotEmpty()
                .NotNull();

            RuleFor(r => r.BidId)
                .NotEmpty()
                .NotNull();

            RuleFor(r => r.Sent)
                .NotEmpty()
                .NotNull();
        }
    }
}
