using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.User.Application.Queries;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Handlers;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(IUserRepository userRepository, ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user == null || !user.IsActive)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}

public class GetUsersByIdsQueryHandler : IRequestHandler<GetUsersByIdsQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersByIdsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByIdsAsync(request.UserIds);
        
        return users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();
    }
}

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public SearchUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchUsersAsync(request.Keyword);
        
        return users.Where(u => u.IsActive).Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();
    }
}

public class ValidateUserQueryHandler : IRequestHandler<ValidateUserQuery, bool>
{
    private readonly IUserRepository _userRepository;

    public ValidateUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        return user != null && user.IsActive;
    }
}
