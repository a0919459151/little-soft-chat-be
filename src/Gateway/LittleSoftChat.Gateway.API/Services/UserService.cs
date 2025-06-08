using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Shared.Infrastructure.Authentication;
using GatewayModels = LittleSoftChat.Gateway.API.Models;
using Grpc.Core;

namespace LittleSoftChat.Gateway.API.Services;

public interface IUserService
{
    Task<GatewayModels.LoginResponse> LoginAsync(GatewayModels.LoginRequest request);
    Task<GatewayModels.LoginResponse> RegisterAsync(GatewayModels.RegisterRequest request);
    Task<GatewayModels.UserProfile?> GetUserProfileAsync(int userId);
    Task<bool> UpdateUserProfileAsync(int userId, GatewayModels.UpdateProfileRequest request);
    Task<List<GatewayModels.UserProfile>> SearchUsersAsync(string searchTerm);
}

public class UserService : IUserService
{
    private readonly LittleSoftChat.Shared.Contracts.UserService.UserServiceClient _userClient;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        LittleSoftChat.Shared.Contracts.UserService.UserServiceClient userClient, 
        IJwtTokenService jwtTokenService,
        ILogger<UserService> logger)
    {
        _userClient = userClient;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public Task<GatewayModels.LoginResponse> LoginAsync(GatewayModels.LoginRequest request)
    {
        try
        {
            // Note: This is a simplified implementation. In a real scenario, you would:
            // 1. Validate credentials against a user store (database, external service, etc.)
            // 2. For now, we'll use the gRPC service to validate if user exists
            
            // First, we need to implement a way to validate credentials
            // Since the gRPC service doesn't have login, we'll return a placeholder
            _logger.LogWarning("Login functionality needs to be implemented with proper credential validation");
            
            return Task.FromResult(new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Login functionality not yet implemented - requires credential validation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Task.FromResult(new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Authentication service unavailable"
            });
        }
    }

    public Task<GatewayModels.LoginResponse> RegisterAsync(GatewayModels.RegisterRequest request)
    {
        try
        {
            // Note: Registration would typically involve:
            // 1. Creating a new user record
            // 2. Hashing password
            // 3. Storing in database
            // Since our gRPC service doesn't expose registration, we'll return a placeholder
            
            _logger.LogWarning("Registration functionality needs to be implemented");
            
            return Task.FromResult(new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Registration functionality not yet implemented"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return Task.FromResult(new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Registration service unavailable"
            });
        }
    }

    public async Task<GatewayModels.UserProfile?> GetUserProfileAsync(int userId)
    {
        try
        {
            var request = new GetUserRequest { UserId = userId };
            var response = await _userClient.GetUserAsync(request);

            return new GatewayModels.UserProfile
            {
                Id = response.Id,
                Username = response.Username,
                DisplayName = response.DisplayName,
                Avatar = response.Avatar,
                IsActive = response.IsActive,
                CreatedAt = DateTime.TryParse(response.CreatedAt, out var createdAt) ? createdAt : DateTime.MinValue,
                // Note: These properties are not available in the current user.proto
                Email = string.Empty, // Not available in user.proto
                FullName = response.DisplayName, // Using DisplayName as fallback
                IsOnline = false, // Not available in user.proto
                LastSeen = null // Not available in user.proto
            };
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting user profile");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return null;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, GatewayModels.UpdateProfileRequest request)
    {
        try
        {
            var grpcRequest = new UpdateUserRequest
            {
                UserId = userId,
                DisplayName = request.DisplayName,
                Avatar = request.Avatar
            };

            var response = await _userClient.UpdateUserAsync(grpcRequest);
            return response.Success;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error updating user profile");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return false;
        }
    }

    public async Task<List<GatewayModels.UserProfile>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            var request = new SearchUsersRequest 
            { 
                Keyword = searchTerm,
                Page = 1,
                PageSize = 50
            };
            var response = await _userClient.SearchUsersAsync(request);

            return response.Users.Select(u => new GatewayModels.UserProfile
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Avatar = u.Avatar,
                IsActive = u.IsActive,
                CreatedAt = DateTime.TryParse(u.CreatedAt, out var createdAt) ? createdAt : DateTime.MinValue,
                // Note: These properties are not available in the current user.proto
                Email = string.Empty, // Not available in user.proto
                FullName = u.DisplayName, // Using DisplayName as fallback
                IsOnline = false, // Not available in user.proto
                LastSeen = null // Not available in user.proto
            }).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error searching users");
            return new List<GatewayModels.UserProfile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return new List<GatewayModels.UserProfile>();
        }
    }
}
