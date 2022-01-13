using FluentValidation;
using Wavelength.Server.WebAPI.Services;

namespace Wavelength.Server.WebAPI.Features.Auctions.CloseAuctionsCommand
{
    public class RequestValidator
        : AbstractValidator<RequestCommand>
    {
        public RequestValidator(ICouchbaseConfigService config)
        {
            RuleFor(r => r.ClosingCode)
                .NotEmpty()
                .NotNull()
                .Equal(config.Config.ClosingCode);
        }
    }
}
