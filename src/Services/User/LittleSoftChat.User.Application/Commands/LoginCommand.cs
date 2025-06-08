using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // First try to find user by username
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            
            // If not found by username, try by email
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(request.Username);
            }
            
            if (user == null || !user.IsActive)
            {
                return LoginResult.Failure("Invalid username or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return LoginResult.Failure("Invalid username or password");
            }

            var tokenInfo = new UserTokenInfo
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            };

            var token = _jwtTokenService.GenerateToken(tokenInfo);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Avatar = user.Avatar,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return LoginResult.Success(token, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return LoginResult.Failure("An error occurred during login");
        }
    }
}
