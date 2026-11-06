using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Deshace la amistad con otra cuenta. Cualquiera de las dos partes puede hacerlo; como la amistad es
    /// simétrica, desaparece de las listas de las dos.
    /// </summary>
    public sealed class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, Result>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        public RemoveFriendCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
        }

        public async ValueTask<Result> Handle(
            RemoveFriendCommand request,
            CancellationToken cancellationToken)
        {
            var account = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (account is null)
                return Result.Failure("The authenticated user has no business account.");

            var friendship = await _friendshipRepository.GetByPairReadOnlyAsync(
                account.Id,
                request.UserId,
                cancellationToken);

            if (friendship is null)
                return Result.Failure("The accounts are not friends.");

            await _friendshipRepository.Delete(friendship);

            return Result.Success();
        }
    }
}
