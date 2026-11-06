using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Rechaza una solicitud pendiente. Solo el destinatario puede hacerlo y no genera nada más: el rechazo
    /// es silencioso y el emisor podrá volver a intentarlo.
    /// </summary>
    public sealed class RejectFriendRequestCommandHandler
        : IRequestHandler<RejectFriendRequestCommand, Result>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;

        public RejectFriendRequestCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendRequestRepository friendRequestRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendRequestRepository = friendRequestRepository;
        }

        public async ValueTask<Result> Handle(
            RejectFriendRequestCommand request,
            CancellationToken cancellationToken)
        {
            var account = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (account is null)
                return Result.Failure("The authenticated user has no business account.");

            var friendRequest = await _friendRequestRepository.GetById(request.RequestId);

            if (friendRequest is null)
                return Result.Failure("The friend request does not exist.");

            if (friendRequest.ReceiverId != account.Id)
                return Result.Failure("Only the recipient can reject this friend request.");

            if (friendRequest.Status != RequestStatus.Pending)
                return Result.Failure("The friend request is not pending.");

            friendRequest.Reject();
            await _friendRequestRepository.Update(friendRequest);

            return Result.Success();
        }
    }
}
