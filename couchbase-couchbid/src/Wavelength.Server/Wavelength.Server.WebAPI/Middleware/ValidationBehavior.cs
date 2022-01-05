/** 
 * https://code-maze.com/cqrs-mediatr-fluentvalidation/
 */
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using System.Linq;

namespace Wavelength.Server.WebAPI.Middleware
{
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
         where TRequest : IRequest<TResponse>

    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public Task<TResponse> Handle(
            TRequest request, 
            CancellationToken cancellationToken, 
            RequestHandlerDelegate<TResponse> next)
        {
			if (!_validators.Any())
			{
				return next();
			}
			var context = new ValidationContext<TRequest>(request);
			var errors = _validators
				.Select(x => x.Validate(context))
				.SelectMany(x => x.Errors)
				.Where(x => x != null)
				.ToList();

			if (errors.Any())
			{
				throw new ValidationException(errors);
			}
			return next();
		}
    }
}
