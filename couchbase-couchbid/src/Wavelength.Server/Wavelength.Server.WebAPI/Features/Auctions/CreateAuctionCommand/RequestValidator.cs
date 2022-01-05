using FluentValidation;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Features.Auctions.CreateAuctionCommand
{
    public class RequestValidator
        : AbstractValidator<RequestCommand>
    {
        public RequestValidator(ICouchbaseConfigService config)
        {
            RuleFor(r => r.Title)
                .NotNull()
                .NotEmpty();

            RuleFor(r => r.ImageUrl)
                .NotNull()
                .NotEmpty();

            RuleFor(r => r.StopTime)
                .NotNull();

            RuleFor(r => r.AuthCode)
                .NotEmpty()
                .NotNull()
                .Equal(config.Config.ClosingCode);
        }
    }
}
