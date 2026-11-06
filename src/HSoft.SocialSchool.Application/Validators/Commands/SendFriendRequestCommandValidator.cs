using FluentValidation;
using HSoft.SocialSchool.Application.Commands.Friends;

namespace HSoft.SocialSchool.Application.Validators.Commands
{
    /// <summary>
    /// Valida la forma del mensaje: sin destinatario no hay solicitud que enviar. Que el identificador sea un
    /// GUID lo garantiza el propio tipo del mensaje —un valor mal formado no llega a enlazarse—, así que aquí
    /// solo se comprueba que venga relleno. Las reglas de negocio las resuelve el handler, que es quien
    /// consulta las cuentas y sus relaciones.
    /// </summary>
    public sealed class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
    {
        public SendFriendRequestCommandValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty().WithMessage("The recipient is required.");
        }
    }
}
