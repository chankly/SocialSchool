using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Services.Session;

namespace HSoft.SocialSchool.Web.Services.Demo;

/// <summary>Datos de la cuenta que simula cada escenario de sesión.</summary>
public sealed record DemoUser(
    Guid Id,
    string Email,
    string DisplayName,
    string? RawUserType,
    AccountType? AccountType,
    AccountStatus? AccountStatus);

/// <summary>Resuelve la cuenta ficticia asociada a un escenario.</summary>
public static class DemoUsers
{
    /// <summary>Devuelve los datos de la cuenta del escenario indicado.</summary>
    public static DemoUser For(DemoSessionScenario scenario) => scenario switch
    {
        DemoSessionScenario.AthleteWithProfile => new DemoUser(
            DemoIds.AthleteWithProfile,
            "ana.otxoa@example.com",
            "Ana Otxoa",
            "Deportista",
            AccountType.Athlete,
            AccountStatus.Active),

        DemoSessionScenario.AthleteWithoutProfile => new DemoUser(
            DemoIds.AthleteWithoutProfile,
            "nuevo.deportista@example.com",
            "Nuevo deportista",
            "Athlete",
            AccountType.Athlete,
            AccountStatus.Active),

        DemoSessionScenario.SchoolWithProfile => new DemoUser(
            DemoIds.SchoolWithProfile,
            "hola@olasdelnorte.example",
            "Olas del Norte",
            "School",
            AccountType.School,
            AccountStatus.Active),

        DemoSessionScenario.SchoolWithoutProfile => new DemoUser(
            DemoIds.SchoolWithoutProfile,
            "escuela.nueva@example.com",
            "Escuela sin validar",
            "Escuela",
            AccountType.School,
            AccountStatus.PendingValidation),

        DemoSessionScenario.SignedOut => new DemoUser(
            Guid.Empty,
            string.Empty,
            "Invitado",
            null,
            null,
            null),

        _ => new DemoUser(Guid.Empty, string.Empty, "Invitado", null, null, null)
    };
}
