using Grpc.Core;
using Microsoft.Extensions.Logging;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Shared.Domain.DTOs;
using LittleSoftChat.Shared.Domain.Exceptions;

namespace LittleSoftChat.Shared.Infrastructure.GrpcClients;

public class UserGrpcClient : IUserGrpcClient
{
    private readonly UserService.UserServiceClient _grpcClient;
    private readonly ILogger<UserGrpcClient> _logger;

    public UserGrpcClient(UserService.UserServiceClient grpcClient, ILogger<UserGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserAsync(int userId)
    {
        try
        {
            var request = new GetUserRequest { UserId = userId };
            var response = await _grpcClient.GetUserAsync(request);
            
            // ACL: 外部模型轉內部模型
            return new UserDto
            {
                Id = response.Id,
                Username = response.Username,
                DisplayName = response.DisplayName,
                Avatar = response.Avatar,
                IsActive = response.IsActive,
                CreatedAt = DateTime.Parse(response.CreatedAt)
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return null;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId} via gRPC", userId);
            throw new ExternalServiceException("User service unavailable", ex);
        }
    }

    public async Task<List<UserDto>> GetUsersAsync(IEnumerable<int> userIds)
    {
        try
        {
            var request = new GetUsersRequest();
            request.UserIds.AddRange(userIds);
            
            var response = await _grpcClient.GetUsersAsync(request);
            
            return response.Users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Avatar = u.Avatar,
                IsActive = u.IsActive,
                CreatedAt = DateTime.Parse(u.CreatedAt)
            }).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to get users via gRPC");
            throw new ExternalServiceException("User service unavailable", ex);
        }
    }

    public async Task<bool> ValidateUserAsync(int userId)
    {
        try
        {
            var request = new ValidateUserRequest { UserId = userId };
            var response = await _grpcClient.ValidateUserAsync(request);
            return response.IsValid;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to validate user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<UserDto>> SearchUsersAsync(string keyword)
    {
        try
        {
            var request = new SearchUsersRequest { Keyword = keyword };
            var response = await _grpcClient.SearchUsersAsync(request);
            
            return response.Users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Avatar = u.Avatar,
                IsActive = u.IsActive,
                CreatedAt = DateTime.Parse(u.CreatedAt)
            }).ToList();
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to search users with keyword {Keyword}", keyword);
            return new List<UserDto>();
        }
    }
}
