using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Friendships;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Crea la solicitud de amistad. Las reglas que cruzan agregados —no a uno mismo, no entre dos escuelas,
    /// no si ya son amigos y no si hay una solicitud pendiente en cualquier dirección— se comprueban aquí,
    /// antes de crear nada: si alguna falla, la base de datos queda intacta.
    /// </summary>
    public sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Result<Guid>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;

        public SendFriendRequestCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IFriendshipRepository friendshipRepository,
            IFriendRequestRepository friendRequestRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _friendRequestRepository = friendRequestRepository;
        }

        public async ValueTask<Result<Guid>> Handle(
            SendFriendRequestCommand request,
            CancellationToken cancellationToken)
        {
            var requester = await _userRepository.GetAuthenticatedAccountAsync(_currentUser, cancellationToken);

            if (requester is null)
                return Result.Failure<Guid>("The authenticated user has no business account.");

            if (requester.Id == request.UserId)
                return Result.Failure<Guid>("A user cannot send a friend request to themselves.");

            var receiver = await _userRepository.GetByIdReadOnly(request.UserId, cancellationToken);

            if (receiver is null)
                return Result.Failure<Guid>("The recipient account does not exist.");

            if (requester.AccountType == AccountType.School && receiver.AccountType == AccountType.School)
                return Result.Failure<Guid>("Two schools cannot be friends.");

            var friendship = await _friendshipRepository.GetByPairReadOnlyAsync(
                requester.Id,
                receiver.Id,
                cancellationToken);

            if (friendship is not null)
                return Result.Failure<Guid>("The accounts are already friends.");

            if (await _friendRequestRepository.ExistsPendingBetweenAsync(requester.Id, receiver.Id, cancellationToken))
                return Result.Failure<Guid>("There is already a pending friend request between the accounts.");

            var friendRequest = FriendRequest.Create(Guid.NewGuid(), requester.Id, receiver.Id);
            await _friendRequestRepository.Add(friendRequest);

            return Result.Success(friendRequest.Id);
        }
    }
}
