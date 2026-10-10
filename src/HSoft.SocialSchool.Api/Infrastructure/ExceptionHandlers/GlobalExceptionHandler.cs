using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Infrastructure.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var problemDetails = BuildProblemDetails(httpContext, exception);

            httpContext.Response.StatusCode = problemDetails.Status ?? 500;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private ProblemDetails BuildProblemDetails(HttpContext context, Exception exception)
        {
            // Base común
            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };

            // Manejar según el tipo
            switch (exception)
            {
                case ValidationException validationEx:
                    LogAndBuildValidationProblem(context, validationEx, problemDetails);
                    break;

                default:
                    LogAndBuildGenericProblem(context, exception, problemDetails);
                    break;
            }

            // Añadir debug info en desarrollo
            if (_environment.IsDevelopment())
            {
                AddDebugInfo(problemDetails, exception);
            }

            return problemDetails;
        }

        private void LogAndBuildValidationProblem(
            HttpContext context,
            ValidationException exception,
            ProblemDetails problemDetails)
        {
            _logger.LogWarning(
                exception,
                "Validation failed for request {Method} {Path}: {@Errors}",
                context.Request.Method,
                context.Request.Path,
                exception.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Validation failed";
            problemDetails.Detail = "One or more validation errors occurred.";
            problemDetails.Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

            var errors = exception.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(
                    g => string.IsNullOrEmpty(g.Key) ? "general" : g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            problemDetails.Extensions["errors"] = errors;
        }

        private void LogAndBuildGenericProblem(
            HttpContext context,
            Exception exception,
            ProblemDetails problemDetails)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred - Request: {Method} {Path}, User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous");

            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "An unexpected error occurred";
            problemDetails.Detail = "An unexpected error occurred while processing your request. Please try again later.";
            problemDetails.Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1";
        }

        private void AddDebugInfo(ProblemDetails problemDetails, Exception exception)
        {
            var debugInfo = new Dictionary<string, object>
            {
                ["message"] = exception.Message,
                ["type"] = exception.GetType().FullName,
                ["stackTrace"] = exception.StackTrace,
                ["source"] = exception.Source
            };

            if (exception.InnerException != null)
            {
                debugInfo["innerException"] = new
                {
                    message = exception.InnerException.Message,
                    type = exception.InnerException.GetType().FullName
                };
            }

            problemDetails.Extensions["debugInfo"] = debugInfo;
        }
    }
}