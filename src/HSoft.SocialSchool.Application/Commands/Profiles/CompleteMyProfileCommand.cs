using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Profiles
{
    /// <summary>
    /// Completa el perfil del usuario autenticado. Trae el nick y <b>exactamente uno</b> de los dos bloques:
    /// el tipo de la cuenta decide cuál corresponde y el cliente no puede cambiarlo.
    /// </summary>
    /// <param name="Nickname">Nick elegido por el usuario; se fija en este momento y ya no cambia.</param>
    /// <param name="Athlete">Datos del deportista, cuando la cuenta es de ese tipo.</param>
    /// <param name="School">Datos de la escuela, cuando la cuenta es de ese tipo.</param>
    public sealed record CompleteMyProfileCommand(
        string Nickname,
        AthleteProfileRequest? Athlete,
        SchoolProfileRequest? School) : IRequest<Result<MyProfileResponse>>;
}
