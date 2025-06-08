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

    public async Task<GatewayModels.LoginResponse> LoginAsync(GatewayModels.LoginRequest request)
    {
        try
        {
            var grpcRequest = new LittleSoftChat.Shared.Contracts.LoginRequest
            {
                Username = request.Email, // Pass email as username - the User Service will handle both
                Password = request.Password
            };

            var grpcResponse = await _userClient.LoginAsync(grpcRequest);
            
            var response = new GatewayModels.LoginResponse
            {
                IsSuccess = grpcResponse.IsSuccess,
                Token = grpcResponse.Token,
                ErrorMessage = grpcResponse.ErrorMessage
            };

            if (grpcResponse.IsSuccess && grpcResponse.User != null)
            {
                response.User = new GatewayModels.UserProfile
                {
                    Id = grpcResponse.User.Id,
                    Username = grpcResponse.User.Username,
                    DisplayName = grpcResponse.User.DisplayName,
                    Avatar = grpcResponse.User.Avatar,
                    IsActive = grpcResponse.User.IsActive,
                    CreatedAt = DateTime.Parse(grpcResponse.User.CreatedAt)
                };
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during login for user {Email}", request.Email);
            return new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Authentication service unavailable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during login"
            };
        }
    }

    public async Task<GatewayModels.LoginResponse> RegisterAsync(GatewayModels.RegisterRequest request)
    {
        try
        {
            var grpcRequest = new LittleSoftChat.Shared.Contracts.RegisterRequest
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.DisplayName
            };

            var grpcResponse = await _userClient.RegisterAsync(grpcRequest);
            
            var response = new GatewayModels.LoginResponse
            {
                IsSuccess = grpcResponse.IsSuccess,
                Token = grpcResponse.Token,
                ErrorMessage = grpcResponse.ErrorMessage
            };

            if (grpcResponse.IsSuccess && grpcResponse.User != null)
            {
                response.User = new GatewayModels.UserProfile
                {
                    Id = grpcResponse.User.Id,
                    Username = grpcResponse.User.Username,
                    DisplayName = grpcResponse.User.DisplayName,
                    Avatar = grpcResponse.User.Avatar,
                    IsActive = grpcResponse.User.IsActive,
                    CreatedAt = DateTime.Parse(grpcResponse.User.CreatedAt)
                };
            }

            return response;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error during registration for user {Username}", request.Username);
            return new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "Registration service unavailable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            return new GatewayModels.LoginResponse
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred during registration"
            };
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
