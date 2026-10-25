using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Services.Demo;

namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Sesión ficticia en memoria. Permite alternar entre deportista y escuela, entre cuenta
/// con perfil y sin perfil, y entre sesión y ausencia de sesión.
/// </summary>
public sealed class MockSessionService : ISessionService
{
    private DemoSessionScenario _scenario = DemoSessionScenario.AthleteWithProfile;
    private DemoUser _user = DemoUsers.For(DemoSessionScenario.AthleteWithProfile);

    /// <inheritdoc />
    public event Action? Changed;

    /// <inheritdoc />
    public DemoSessionScenario Scenario => _scenario;

    /// <inheritdoc />
    public bool IsAuthenticated => _scenario != DemoSessionScenario.SignedOut;

    /// <inheritdoc />
    public Guid UserId => _user.Id;

    /// <inheritdoc />
    public string Email => _user.Email;

    /// <inheritdoc />
    public string DisplayName => _user.DisplayName;

    /// <inheritdoc />
    public AccountType? AccountType => _user.AccountType;

    /// <inheritdoc />
    public AccountStatus? AccountStatus => _user.AccountStatus;

    /// <inheritdoc />
    public string? RawUserType => _user.RawUserType;

    /// <inheritdoc />
    public void UseScenario(DemoSessionScenario scenario)
    {
        if (_scenario == scenario)
        {
            return;
        }

        _scenario = scenario;
        _user = DemoUsers.For(scenario);
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void Reset() => UseScenario(DemoSessionScenario.AthleteWithProfile);
}
