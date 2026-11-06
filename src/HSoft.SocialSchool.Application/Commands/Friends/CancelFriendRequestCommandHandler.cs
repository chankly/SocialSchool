using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Retira una solicitud enviada. Solo su emisor puede cancelarla y solo mientras siga pendiente; un
    /// tercero ajeno no puede resolverla.
    /// </summary>
    public sealed class CancelFriendRequestCommandHandler
        : IRequestHandler<CancelFriendRequestCommand, Result>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;

        public CancelFriendRequestCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendRequestRepository friendRequestRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendRequestRepository = friendRequestRepository;
        }

        public async ValueTask<Result> Handle(
            CancelFriendRequestCommand request,
            CancellationToken cancellationToken)
        {
            var account = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (account is null)
                return Result.Failure("The authenticated user has no business account.");

            var friendRequest = await _friendRequestRepository.GetById(request.RequestId);

            if (friendRequest is null)
                return Result.Failure("The friend request does not exist.");

            if (friendRequest.RequesterId != account.Id)
                return Result.Failure("Only the sender can cancel this friend request.");

            if (friendRequest.Status != RequestStatus.Pending)
                return Result.Failure("The friend request is not pending.");

            friendRequest.Cancel();
            await _friendRequestRepository.Update(friendRequest);

            return Result.Success();
        }
    }
}
