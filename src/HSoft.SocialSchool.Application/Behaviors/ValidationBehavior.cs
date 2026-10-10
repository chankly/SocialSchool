using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Serilog;
using System.Diagnostics;

namespace HSoft.SocialSchool.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(ILogger logger, IEnumerable<IValidator<TRequest>> validators)
        {
            _logger = logger;
            _validators = validators;
        }

        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.Debug($"Validating {typeof(TRequest).Name}");

            if (!_validators.Any())
            {
                return await next(request, cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);
            var validationResults = new List<FluentValidation.Results.ValidationResult>();

            foreach (var validator in _validators)
            {
                validationResults.Add(await validator.ValidateAsync(context, cancellationToken));
            }

            var errorsDictionary = validationResults.SelectMany(x => x.Errors)
                .Where(x => x != null)
                .Select(s => new ValidationFailure(s.PropertyName, s.ErrorMessage));

            if (errorsDictionary.Any())
            {
                throw new ValidationException(typeof(TRequest).Name, errorsDictionary);
            }

            sw.Stop();
            _logger.Debug($"Validated {typeof(TRequest).Name} in [{sw.ElapsedMilliseconds}] ms");

            return await next(request, cancellationToken);
        }
    }
}
