using MediatR;
using Microsoft.Extensions.Logging;
using LittleSoftChat.User.Application.Commands;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Domain.Results;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.User.Domain.Entities;

namespace LittleSoftChat.User.Application.Handlers;

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
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            
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
            var user = new LittleSoftChat.User.Domain.Entities.UserEntity
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

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(IUserRepository userRepository, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            user.DisplayName = request.DisplayName;
            user.Avatar = request.Avatar;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);
            
            if (result)
            {
                _logger.LogInformation("User profile updated for user {UserId}", request.UserId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
            return false;
        }
    }
}
