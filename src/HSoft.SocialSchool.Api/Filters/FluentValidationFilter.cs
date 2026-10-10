using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HSoft.SocialSchool.Api.Filters
{
    public sealed class FluentValidationFilter(
        IServiceProvider serviceProvider,
        ProblemDetailsFactory problemDetailsFactory) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Solo procesar si hay argumentos
            if (context.ActionArguments.Count == 0)
            {
                await next();
                return;
            }

            // Validar cada argumento
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = serviceProvider.GetService(validatorType) as IValidator;

                if (validator is null) continue;

                var validationContext = new ValidationContext<object>(arg);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    context.Result = CreateValidationProblemDetails(
                        context.HttpContext,
                        validationResult.Errors);
                    return;
                }
            }

            await next();
        }

        private IActionResult CreateValidationProblemDetails(
            HttpContext httpContext,
            IEnumerable<FluentValidation.Results.ValidationFailure> errors)
        {
            var modelState = new ModelStateDictionary();

            foreach (var failure in errors)
            {
                modelState.AddModelError(
                    string.IsNullOrEmpty(failure.PropertyName) ? "general" : failure.PropertyName,
                    failure.ErrorMessage);
            }

            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                httpContext,
                modelState,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: "One or more validation errors occurred.",
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");

            return new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}
