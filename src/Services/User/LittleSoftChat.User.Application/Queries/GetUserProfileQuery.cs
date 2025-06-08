using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Queries;

// Query Definition
public record GetUserProfileQuery(int UserId) : IRequest<UserDto?>;

// Validator
public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

// Handler
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
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("User profile not found or inactive for UserId: {UserId}", request.UserId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for UserId: {UserId}", request.UserId);
            throw;
        }
    }
}
