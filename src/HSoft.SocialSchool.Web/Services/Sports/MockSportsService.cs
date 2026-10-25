using HSoft.SocialSchool.Web.Services.Sports.Responses;

namespace HSoft.SocialSchool.Web.Services.Sports;

/// <summary>Catálogo de deportes simulado con una pequeña latencia de red.</summary>
public sealed class MockSportsService : ISportsService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<SportResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(120, cancellationToken);
        return SportCatalog.All;
    }

    /// <inheritdoc />
    public async Task<SportResponse?> GetByIdAsync(Guid sportId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(80, cancellationToken);
        return SportCatalog.Find(sportId);
    }
}
