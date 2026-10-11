using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text;

namespace HSoft.SocialSchool.Api.Filters
{
    public class PerformanceAuditFilter : ActionFilterAttribute
    {
        private const string SubjectClaimType = "sub";
        private const string KeycloakClientClaimType = "azp";
        private const string GenericClientClaimType = "client_id";

        private readonly ILogger _logger;
        private readonly bool _auditEnabled;

        private Stopwatch _stopwatch = null!;
        private DateTime _startMoment;

        public PerformanceAuditFilter(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger("Audit");
            bool.TryParse(configuration["AuditingEnabled"], out _auditEnabled);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
            _startMoment = DateTime.UtcNow;

            var request = context.HttpContext.Request;
            var method = request.Method;
            var path = request.Path;
            var hasAuth = request.Headers.ContainsKey("Authorization");

            if (IsReadMethod(method))
            {
                var queryParams = request.Query
                    .ToDictionary(k => k.Key, v => v.Value.ToString());

                _logger.LogDebug(
                    "Incoming {Method} {Path} | Auth: {HasAuth} | Params: {@QueryParams}",
                    method, path, hasAuth ? "yes" : "no", queryParams);
            }
            else
            {
                var body = ReadBodySafe(request);

                _logger.LogDebug(
                    "Incoming {Method} {Path} | Auth: {HasAuth} | Body: {Body}",
                    method, path, hasAuth ? "yes" : "no", body);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.OnCompleted(async () =>
            {
                if (context.ExceptionHandled) return;

                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;
                _stopwatch.Stop();

                if (response.StatusCode >= 500)
                    _logger.LogError(
                        "Completed {Method} {Path} | {StatusCode} {StatusText} | {ElapsedMs}ms",
                        request.Method, request.Path,
                        response.StatusCode, GetStatusText(response.StatusCode),
                        _stopwatch.ElapsedMilliseconds);
                else if (response.StatusCode >= 400)
                    _logger.LogWarning(
                        "Completed {Method} {Path} | {StatusCode} {StatusText} | {ElapsedMs}ms",
                        request.Method, request.Path,
                        response.StatusCode, GetStatusText(response.StatusCode),
                        _stopwatch.ElapsedMilliseconds);
                else
                    _logger.LogDebug(
                        "Completed {Method} {Path} | {StatusCode} {StatusText} | {ElapsedMs}ms",
                        request.Method, request.Path,
                        response.StatusCode, GetStatusText(response.StatusCode),
                        _stopwatch.ElapsedMilliseconds);

                if (_auditEnabled)
                {
                    var (subject, clientId) = ExtractTokenClaims(context.HttpContext);

                    _logger.LogInformation(
                        "AUDIT | User: {Subject} | Client: {ClientId} | {Method} {Path} | Status: {StatusCode} | Start: {StartTime:O} | End: {EndTime:O} | Elapsed: {ElapsedMs}ms",
                        subject, clientId,
                        request.Method, request.Path,
                        response.StatusCode,
                        _startMoment, DateTime.UtcNow,
                        _stopwatch.ElapsedMilliseconds);
                }

                await Task.CompletedTask;
            });
        }

        private static bool IsReadMethod(string method) =>
            HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

        private static string GetStatusText(int statusCode) => statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            422 => "Unprocessable Entity",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            _ => string.Empty
        };

        private static string ReadBodySafe(HttpRequest request)
        {
            try
            {
                if (!request.Body.CanSeek || request.ContentLength is null or 0)
                    return "(no body)";

                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = reader.ReadToEnd();
                request.Body.Position = 0;
                return body;
            }
            catch
            {
                return "(unreadable)";
            }
        }

        // Reads the identity from the authenticated principal instead of decoding the token by hand,
        // so the audit entry no longer depends on the token format.
        private static (string subject, string clientId) ExtractTokenClaims(HttpContext httpContext)
        {
            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                return ("anonymous", "unknown");

            var subject = user.FindFirst(SubjectClaimType)?.Value ?? "unknown";

            // Keycloak reports the calling client in "azp"; other providers use "client_id".
            var clientId = user.FindFirst(KeycloakClientClaimType)?.Value
                           ?? user.FindFirst(GenericClientClaimType)?.Value
                           ?? "unknown";

            return (subject, clientId);
        }
    }
}
