using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Queries;

// Query Definition
public record GetUsersByIdsQuery(List<int> UserIds) : IRequest<List<UserDto>>;

// Validator
public class GetUsersByIdsQueryValidator : AbstractValidator<GetUsersByIdsQuery>
{
    public GetUsersByIdsQueryValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull()
            .WithMessage("UserIds cannot be null")
            .NotEmpty()
            .WithMessage("UserIds cannot be empty")
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot request more than 100 users at once")
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("All UserIds must be greater than 0");
    }
}

// Handler
public class GetUsersByIdsQueryHandler : IRequestHandler<GetUsersByIdsQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersByIdsQueryHandler> _logger;

    public GetUsersByIdsQueryHandler(IUserRepository userRepository, ILogger<GetUsersByIdsQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetByIdsAsync(request.UserIds);
            
            _logger.LogInformation("Retrieved {Count} users out of {Requested} requested", 
                users.Count, request.UserIds.Count);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by IDs: {UserIds}", string.Join(",", request.UserIds));
            throw;
        }
    }
}
