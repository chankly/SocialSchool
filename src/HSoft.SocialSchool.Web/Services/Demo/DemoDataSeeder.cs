using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Models.Friends;
using HSoft.SocialSchool.Web.Services.Friends.Responses;
using HSoft.SocialSchool.Web.Services.Profile.Responses;
using HSoft.SocialSchool.Web.Services.Sports;

namespace HSoft.SocialSchool.Web.Services.Demo;

/// <summary>
/// Datos ficticios con los que arranca la maqueta. Se han escrito a mano para que las
/// pantallas se parezcan a un producto real sin depender del backend.
/// </summary>
internal static class DemoDataSeeder
{
    public static void Seed(MockDataStore store)
    {
        SeedProfiles(store);
        SeedFriendships(store);
    }

    private static void SeedProfiles(MockDataStore store)
    {
        var athleteProfile = new ProfileResponse(
            DemoIds.AthleteWithProfile,
            "anaotxoa",
            "ana.otxoa@example.com",
            AccountType.Athlete,
            AccountStatus.Active,
            new AthleteProfileResponse(
                "Ana",
                "Otxoa",
                new DateOnly(1994, 6, 12),
                "ES",
                "San Sebastián",
                null,
                "Surfista y skater. Entre Zurriola y La Kantera, siempre buscando la siguiente ola o el siguiente spot.",
                [
                    new SportSummary(SportCatalog.SurfId, "Surf"),
                    new SportSummary(SportCatalog.SkateboardingId, "Skateboarding")
                ]),
            null);

        var schoolProfile = new ProfileResponse(
            DemoIds.SchoolWithProfile,
            "olasdelnorte",
            "hola@olasdelnorte.example",
            AccountType.School,
            AccountStatus.Active,
            null,
            new SchoolProfileResponse(
                "Olas del Norte",
                null,
                SportCatalog.SurfId,
                "Escuela de surf familiar en la playa de Zurriola. Clases de iniciación, perfeccionamiento y surf adaptado durante todo el año.",
                "ES",
                "San Sebastián",
                "hola@olasdelnorte.example",
                "+34 943 555 010",
                "https://olasdelnorte.example"));

        store.Profiles[athleteProfile.Id] = athleteProfile;
        store.Profiles[schoolProfile.Id] = schoolProfile;
        store.ReserveNickname(athleteProfile.Nickname);
        store.ReserveNickname(schoolProfile.Nickname);
    }

    private static void SeedFriendships(MockDataStore store)
    {
        store.Friends.AddRange(
        [
            new FriendResponse(
                DemoIds.MartaElorza, "Marta Elorza", "martaelorza", null,
                AccountType.Athlete, "Surf · Zarautz"),
            new FriendResponse(
                DemoIds.IkerLanda, "Iker Landa", "ikerlanda", null,
                AccountType.Athlete, "Skateboarding · Bilbao"),
            new FriendResponse(
                DemoIds.ZarautzWaves, "Zarautz Waves", "zarautzwaves", null,
                AccountType.School, "Surf · Zarautz")
        ]);

        store.Requests.AddRange(
        [
            new FriendRequestResponse(
                DemoIds.IncomingRequestFromNerea, DemoIds.NereaEtxeberria,
                "Nerea Etxeberria", "nereaetx", null, AccountType.Athlete,
                "Surf · Getxo", DateTimeOffset.UtcNow.AddDays(-1), FriendRequestDirection.Incoming),
            new FriendRequestResponse(
                DemoIds.IncomingRequestFromBilbao, DemoIds.BilbaoSkateLab,
                "Bilbao Skate Lab", "bilbaoskatelab", null, AccountType.School,
                "Skateboarding · Bilbao", DateTimeOffset.UtcNow.AddDays(-3), FriendRequestDirection.Incoming),
            new FriendRequestResponse(
                DemoIds.OutgoingRequestToJon, DemoIds.JonAranburu,
                "Jon Aranburu", "jonaranburu", null, AccountType.Athlete,
                "Surf · Mundaka", DateTimeOffset.UtcNow.AddDays(-2), FriendRequestDirection.Outgoing)
        ]);

        store.People.AddRange(
        [
            new DiscoverablePersonResponse(
                DemoIds.AneZubia, "Ane Zubia", "anezubia", AccountType.Athlete, "Surf · Donostia"),
            new DiscoverablePersonResponse(
                DemoIds.LucasFerreira, "Lucas Ferreira", "lucasferreira", AccountType.Athlete, "Skateboarding · Vigo"),
            new DiscoverablePersonResponse(
                DemoIds.DonostiaClimbClub, "Donostia Climb Club", "donostiaclimb", AccountType.School, "Escalada · Donostia"),
            new DiscoverablePersonResponse(
                DemoIds.MundakaSurfHouse, "Mundaka Surf House", "mundakasurf", AccountType.School, "Surf · Mundaka"),
            new DiscoverablePersonResponse(
                DemoIds.SierraNorteAventura, "Sierra Norte Aventura", "sierranorte", AccountType.School, "Escalada · Madrid"),
            new DiscoverablePersonResponse(
                DemoIds.SchoolWithProfile, "Olas del Norte", "olasdelnorte", AccountType.School, "Surf · San Sebastián")
        ]);
    }
}
