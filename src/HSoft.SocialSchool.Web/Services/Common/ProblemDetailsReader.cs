using System.Net.Http.Json;
using System.Text.Json;

namespace HSoft.SocialSchool.Web.Services.Common;

/// <summary>
/// Extrae el <c>detail</c> de un <c>ProblemDetails</c> del API. El servidor escribe ahí
/// el mensaje de negocio ("ya sois amigos", "no puedes invitarte a ti mismo"), así que es
/// lo único que se muestra al usuario.
/// </summary>
public static class ProblemDetailsReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Devuelve el <c>detail</c> o <see langword="null"/> si la respuesta no lo trae.</summary>
    public static async Task<string?> ReadDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions, cancellationToken);
            return string.IsNullOrWhiteSpace(problem?.Detail) ? null : problem.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record ProblemBody(string? Title, string? Detail);
}
