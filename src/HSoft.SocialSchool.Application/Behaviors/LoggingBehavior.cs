using Mediator;
using Serilog;
using System.Diagnostics;

namespace HSoft.SocialSchool.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;
        public LoggingBehavior(ILogger logger)
            => _logger = logger;

        public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            _logger.Debug($"Handling {typeof(TRequest).Name}");
            var response = await next(message, cancellationToken);
            sw.Stop();

            _logger.Debug($"Finished handled {typeof(TRequest).Name} in [{sw.ElapsedMilliseconds}] ms");

            return response;
        }
    }
}
