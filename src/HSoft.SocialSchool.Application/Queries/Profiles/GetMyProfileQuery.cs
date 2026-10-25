using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Profiles
{
    /// <summary>
    /// Consulta el perfil del usuario autenticado. El resultado es <see langword="null"/> cuando todavía no
    /// existe perfil, que es un estado normal: la cuenta puede no haberse materializado o estar sin rellenar.
    /// </summary>
    public sealed record GetMyProfileQuery : IRequest<Result<MyProfileResponse?>>;
}
