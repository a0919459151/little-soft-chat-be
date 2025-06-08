using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Shared.Infrastructure.GrpcClients;

public interface IUserGrpcClient
{
    Task<UserDto?> GetUserAsync(int userId);
    Task<List<UserDto>> GetUsersAsync(IEnumerable<int> userIds);
    Task<bool> ValidateUserAsync(int userId);
    Task<List<UserDto>> SearchUsersAsync(string keyword);
}
