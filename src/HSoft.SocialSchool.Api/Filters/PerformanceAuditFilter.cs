using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace HSoft.SocialSchool.Api.Filters
{
    public class PerformanceAuditFilter : ActionFilterAttribute
    {
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
                    var (subject, clientId) = ExtractTokenClaims(request);

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

        private static (string subject, string clientId) ExtractTokenClaims(HttpRequest request)
        {
            try
            {
                if (!request.Headers.TryGetValue("Authorization", out var authHeader))
                    return ("anonymous", "unknown");

                var parts = authHeader.ToString().Split('.');
                if (parts.Length != 3)
                    return ("unknown", "unknown");

                var payload = parts[1];
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var token = JsonSerializer.Deserialize<TokenPayload>(decoded);

                return (token?.Sub ?? "unknown", token?.ClientId ?? "unknown");
            }
            catch
            {
                return ("unknown", "unknown");
            }
        }

        private sealed class TokenPayload
        {
            [System.Text.Json.Serialization.JsonPropertyName("sub")]
            public string? Sub { get; init; }

            [System.Text.Json.Serialization.JsonPropertyName("client_id")]
            public string? ClientId { get; init; }
        }
    }
}