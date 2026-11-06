using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Acepta una solicitud pendiente. Solo el destinatario puede resolverla y solo mientras siga pendiente;
    /// la amistad se crea aquí y se guarda junto con la solicitud aceptada, en una sola escritura.
    /// </summary>
    public sealed class AcceptFriendRequestCommandHandler
        : IRequestHandler<AcceptFriendRequestCommand, Result>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        public AcceptFriendRequestCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendRequestRepository friendRequestRepository,
            IFriendshipRepository friendshipRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendRequestRepository = friendRequestRepository;
            _friendshipRepository = friendshipRepository;
        }

        public async ValueTask<Result> Handle(
            AcceptFriendRequestCommand request,
            CancellationToken cancellationToken)
        {
            var account = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (account is null)
                return Result.Failure("The authenticated user has no business account.");

            var friendRequest = await _friendRequestRepository.GetById(request.RequestId);

            if (friendRequest is null)
                return Result.Failure("The friend request does not exist.");

            if (friendRequest.ReceiverId != account.Id)
                return Result.Failure("Only the recipient can accept this friend request.");

            if (friendRequest.Status != RequestStatus.Pending)
                return Result.Failure("The friend request is not pending.");

            friendRequest.Accept();

            var friendship = Friendship.Create(Guid.NewGuid(), friendRequest.RequesterId, friendRequest.ReceiverId);
            await _friendshipRepository.AddWithRequestResolutionAsync(friendship, friendRequest, cancellationToken);

            return Result.Success();
        }
    }
}
