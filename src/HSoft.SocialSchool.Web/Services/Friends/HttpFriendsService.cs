using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Models.Friends;
using HSoft.SocialSchool.Web.Services.Common;
using HSoft.SocialSchool.Web.Services.Friends.Requests;
using HSoft.SocialSchool.Web.Services.Friends.Responses;

namespace HSoft.SocialSchool.Web.Services.Friends;

/// <summary>
/// Amistades contra <c>/api/friends</c>. La API solo publica <c>id</c>, <c>nickname</c> y
/// <c>accountType</c> de cada cuenta, así que el nick es también el nombre visible.
/// Los <c>400</c> se devuelven con el <c>detail</c> del servidor para pintarlo tal cual.
/// </summary>
public sealed class HttpFriendsService(HttpClient httpClient) : IFriendsService
{
    private const string OverviewEndpoint = "api/friends";
    private const string RequestsEndpoint = "api/friends/requests";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <inheritdoc />
    public async Task<FriendsOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(OverviewEndpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await ProblemDetailsReader.ReadDetailAsync(response, cancellationToken);
            throw new HttpRequestException(detail ?? "No se han podido cargar tus amistades.");
        }

        var body = await response.Content.ReadFromJsonAsync<FriendsOverviewBody>(JsonOptions, cancellationToken)
            ?? throw new HttpRequestException("La API ha devuelto una respuesta inesperada al cargar tus amistades.");

        return MapOverview(body);
    }

    /// <inheritdoc />
    public Task<FriendActionResult> AcceptRequestAsync(Guid requestId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            HttpMethod.Post,
            $"{RequestsEndpoint}/{requestId}/accept",
            body: null,
            "No se ha podido aceptar la solicitud.",
            cancellationToken);

    /// <inheritdoc />
    public Task<FriendActionResult> RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            HttpMethod.Post,
            $"{RequestsEndpoint}/{requestId}/reject",
            body: null,
            "No se ha podido rechazar la solicitud.",
            cancellationToken);

    /// <inheritdoc />
    public Task<FriendActionResult> CancelRequestAsync(Guid requestId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            HttpMethod.Delete,
            $"{RequestsEndpoint}/{requestId}",
            body: null,
            "No se ha podido cancelar la solicitud.",
            cancellationToken);

    /// <inheritdoc />
    public Task<FriendActionResult> RemoveFriendAsync(Guid friendId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            HttpMethod.Delete,
            $"api/friends/{friendId}",
            body: null,
            "No se ha podido deshacer la amistad.",
            cancellationToken);

    /// <inheritdoc />
    public Task<FriendActionResult> SendRequestAsync(Guid personId, CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            HttpMethod.Post,
            RequestsEndpoint,
            new SendFriendRequest(personId),
            "No se ha podido enviar la solicitud.",
            cancellationToken);

    private async Task<FriendActionResult> ExecuteAsync(
        HttpMethod method,
        string endpoint,
        object? body,
        string fallbackError,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, endpoint);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return FriendActionResult.Ok();
            }

            var detail = await ProblemDetailsReader.ReadDetailAsync(response, cancellationToken);
            return FriendActionResult.Fail(string.IsNullOrWhiteSpace(detail) ? fallbackError : detail);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return FriendActionResult.Fail($"{fallbackError} Comprueba la conexión e inténtalo de nuevo.");
        }
    }

    private static FriendsOverviewResponse MapOverview(FriendsOverviewBody body) => new(
        (body.Friends ?? [])
            .Select(person => new FriendResponse(
                person.Id,
                person.Nickname,
                person.Nickname,
                null,
                person.AccountType,
                person.AccountType.ToDisplayName()))
            .ToList(),
        (body.IncomingRequests ?? [])
            .Where(request => request.Requester is not null)
            .Select(request => MapRequest(request, request.Requester!, FriendRequestDirection.Incoming))
            .ToList(),
        (body.OutgoingRequests ?? [])
            .Where(request => request.Receiver is not null)
            .Select(request => MapRequest(request, request.Receiver!, FriendRequestDirection.Outgoing))
            .ToList(),
        (body.Discoverable ?? [])
            .Select(person => new DiscoverablePersonResponse(
                person.Id,
                person.Nickname,
                person.Nickname,
                person.AccountType,
                person.City ?? string.Empty))
            .ToList());

    private static FriendRequestResponse MapRequest(
        FriendRequestBody request,
        PersonBody person,
        FriendRequestDirection direction) => new(
        request.Id,
        person.Id,
        person.Nickname,
        person.Nickname,
        null,
        person.AccountType,
        person.AccountType.ToDisplayName(),
        request.RequestedAt,
        direction);

    private sealed record FriendsOverviewBody(
        List<PersonBody>? Friends,
        List<FriendRequestBody>? IncomingRequests,
        List<FriendRequestBody>? OutgoingRequests,
        List<PersonBody>? Discoverable);

    private sealed record FriendRequestBody(
        Guid Id,
        PersonBody? Requester,
        PersonBody? Receiver,
        DateTimeOffset RequestedAt);

    private sealed record PersonBody(
        Guid Id,
        string Nickname,
        AccountType AccountType,
        string? City);
}
