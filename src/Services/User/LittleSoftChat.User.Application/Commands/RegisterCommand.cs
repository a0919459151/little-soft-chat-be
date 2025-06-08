using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.User.Domain.Entities;

namespace LittleSoftChat.User.Application.Commands;

public record RegisterCommand(string Username, string Email, string Password, string DisplayName) : IRequest<LoginResult>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
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

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one number");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required")
            .MinimumLength(2)
            .WithMessage("DisplayName must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("DisplayName cannot exceed 100 characters");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 檢查用戶名是否已存在
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUserByUsername != null)
            {
                return LoginResult.Failure("Username already exists");
            }

            // 檢查電子郵件是否已存在
            var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return LoginResult.Failure("Email already exists");
            }

            // 創建新用戶
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new UserEntity
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                DisplayName = request.DisplayName,
                Avatar = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userId = await _userRepository.CreateAsync(user);
            user.Id = userId;

            // 生成 JWT Token
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

            _logger.LogInformation("User {Username} registered successfully", user.Username);
            return LoginResult.Success(token, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return LoginResult.Failure("An error occurred during registration");
        }
    }
}
