using FluentValidation;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Features.Auctions.DeleteAuctionCommand
{
    public class RequestValidator
        : AbstractValidator<RequestCommand>
    {
        public RequestValidator(ICouchbaseConfigService config)
        {
            RuleFor(r => r.Id)
                .NotNull();

            RuleFor(r => r.AuthCode)
                .NotEmpty()
                .NotNull()
                .Equal(config.Config.ClosingCode);
        }
    }
}
