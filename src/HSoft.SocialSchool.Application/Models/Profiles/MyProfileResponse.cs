using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>
    /// Perfil del usuario autenticado. El <paramref name="AccountType"/> indica cuál de los dos bloques
    /// viene relleno: solo uno de ellos puede tener valor.
    /// </summary>
    /// <param name="Id">Identificador de la cuenta, que es el mismo que el del perfil.</param>
    /// <param name="Nickname">Nick de la cuenta.</param>
    /// <param name="Email">Correo de la cuenta, tal y como lo aporta el proveedor de identidad.</param>
    /// <param name="AccountType">Tipo de la cuenta, inmutable.</param>
    /// <param name="AccountStatus">Estado de la cuenta; una escuela puede estar pendiente de validación.</param>
    /// <param name="Athlete">Datos del deportista, cuando la cuenta es de ese tipo.</param>
    /// <param name="School">Datos de la escuela, cuando la cuenta es de ese tipo.</param>
    public sealed record MyProfileResponse(
        Guid Id,
        string Nickname,
        string Email,
        AccountType AccountType,
        AccountStatus AccountStatus,
        AthleteProfileResponse? Athlete,
        SchoolProfileResponse? School);
}
