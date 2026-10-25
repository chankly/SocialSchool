using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Models.Friends;
using HSoft.SocialSchool.Web.Services.Demo;
using HSoft.SocialSchool.Web.Services.Friends.Responses;
using HSoft.SocialSchool.Web.Services.Session;

namespace HSoft.SocialSchool.Web.Services.Friends;

/// <summary>
/// Amistades simuladas en memoria. Aplica la regla de que dos escuelas nunca pueden ser
/// amigas y mantiene actualizadas las listas después de cada operación.
/// </summary>
public sealed class MockFriendsService(
    MockDataStore dataStore,
    ISessionService session) : IFriendsService
{
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(220);

    /// <inheritdoc />
    public async Task<FriendsOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(Latency, cancellationToken);

        if (!session.IsAuthenticated)
        {
            return new FriendsOverviewResponse([], [], [], []);
        }

        var isSchoolSession = session.AccountType == AccountType.School;

        var friends = dataStore.Friends
            .Where(friend => friend.Id != session.UserId)
            .Where(friend => !isSchoolSession || friend.AccountType == AccountType.Athlete)
            .OrderBy(friend => friend.DisplayName)
            .ToList();

        var requests = dataStore.Requests
            .Where(request => !isSchoolSession || request.AccountType == AccountType.Athlete)
            .ToList();

        var suggestions = dataStore.People
            .Where(person => person.Id != session.UserId)
            .Where(person => !isSchoolSession || person.AccountType == AccountType.Athlete)
            .OrderBy(person => person.DisplayName)
            .ToList();

        return new FriendsOverviewResponse(
            friends,
            requests.Where(request => request.Direction == FriendRequestDirection.Incoming).ToList(),
            requests.Where(request => request.Direction == FriendRequestDirection.Outgoing).ToList(),
            suggestions);
    }

    /// <inheritdoc />
    public async Task<FriendActionResult> AcceptRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);

        if (!session.IsAuthenticated)
        {
            return FriendActionResult.Fail("No hay una sesión activa.");
        }

        var request = dataStore.Requests.FirstOrDefault(item =>
            item.Id == requestId && item.Direction == FriendRequestDirection.Incoming);

        if (request is null)
        {
            return FriendActionResult.Fail("La solicitud ya no está disponible.");
        }

        if (session.AccountType == AccountType.School && request.AccountType == AccountType.School)
        {
            return FriendActionResult.Fail("Las escuelas no pueden ser amigas entre sí.");
        }

        dataStore.Requests.Remove(request);
        AddFriendIfMissing(request);
        return FriendActionResult.Ok();
    }

    /// <inheritdoc />
    public async Task<FriendActionResult> RejectRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);

        var request = dataStore.Requests.FirstOrDefault(item =>
            item.Id == requestId && item.Direction == FriendRequestDirection.Incoming);

        if (request is null)
        {
            return FriendActionResult.Fail("La solicitud ya no está disponible.");
        }

        dataStore.Requests.Remove(request);
        AddToSuggestionsIfMissing(new DiscoverablePersonResponse(
            request.PersonId,
            request.DisplayName,
            request.Nickname,
            request.AccountType,
            request.Subtitle));

        return FriendActionResult.Ok();
    }

    /// <inheritdoc />
    public async Task<FriendActionResult> CancelRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);

        var request = dataStore.Requests.FirstOrDefault(item =>
            item.Id == requestId && item.Direction == FriendRequestDirection.Outgoing);

        if (request is null)
        {
            return FriendActionResult.Fail("La solicitud ya no está disponible.");
        }

        dataStore.Requests.Remove(request);
        AddToSuggestionsIfMissing(new DiscoverablePersonResponse(
            request.PersonId,
            request.DisplayName,
            request.Nickname,
            request.AccountType,
            request.Subtitle));

        return FriendActionResult.Ok();
    }

    /// <inheritdoc />
    public async Task<FriendActionResult> RemoveFriendAsync(Guid friendId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);

        var friend = dataStore.Friends.FirstOrDefault(item => item.Id == friendId);
        if (friend is null)
        {
            return FriendActionResult.Fail("Esa amistad ya no existe.");
        }

        dataStore.Friends.Remove(friend);
        AddToSuggestionsIfMissing(new DiscoverablePersonResponse(
            friend.Id,
            friend.DisplayName,
            friend.Nickname,
            friend.AccountType,
            friend.Subtitle));

        return FriendActionResult.Ok();
    }

    /// <inheritdoc />
    public async Task<FriendActionResult> SendRequestAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(160, cancellationToken);

        if (!session.IsAuthenticated)
        {
            return FriendActionResult.Fail("Inicia sesión para enviar solicitudes.");
        }

        if (dataStore.Friends.Any(friend => friend.Id == personId))
        {
            return FriendActionResult.Fail("Esta cuenta ya es tu amiga.");
        }

        if (dataStore.Requests.Any(request => request.PersonId == personId))
        {
            return FriendActionResult.Fail("Ya existe una solicitud con esta cuenta.");
        }

        var person = dataStore.People.FirstOrDefault(item => item.Id == personId);
        if (person is null)
        {
            return FriendActionResult.Fail("Esa cuenta ya no aparece en las sugerencias.");
        }

        if (person.Id == session.UserId)
        {
            return FriendActionResult.Fail("No puedes enviarte una solicitud a ti mismo.");
        }

        if (session.AccountType == AccountType.School && person.AccountType == AccountType.School)
        {
            return FriendActionResult.Fail("Las escuelas no pueden ser amigas entre sí.");
        }

        dataStore.People.Remove(person);
        dataStore.Requests.Add(new FriendRequestResponse(
            Guid.NewGuid(),
            person.Id,
            person.DisplayName,
            person.Nickname,
            null,
            person.AccountType,
            person.Subtitle,
            DateTimeOffset.UtcNow,
            FriendRequestDirection.Outgoing));

        return FriendActionResult.Ok();
    }

    private void AddFriendIfMissing(FriendRequestResponse request)
    {
        if (dataStore.Friends.Any(friend => friend.Id == request.PersonId))
        {
            return;
        }

        dataStore.Friends.Add(new FriendResponse(
            request.PersonId,
            request.DisplayName,
            request.Nickname,
            request.PictureUrl,
            request.AccountType,
            request.Subtitle));
    }

    private void AddToSuggestionsIfMissing(DiscoverablePersonResponse person)
    {
        if (dataStore.People.Any(item => item.Id == person.Id))
        {
            return;
        }

        dataStore.People.Add(person);
    }
}
