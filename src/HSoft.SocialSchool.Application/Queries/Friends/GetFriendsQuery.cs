using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Friends;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Friends
{
    /// <summary>
    /// Consulta el resumen de amistades del usuario autenticado: amigos, solicitudes recibidas y enviadas,
    /// y las cuentas a las que todavía puede invitar.
    /// </summary>
    public sealed record GetFriendsQuery : IRequest<Result<FriendsResponse>>;
}
