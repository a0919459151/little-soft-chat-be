using LittleSoftChat.User.Domain.Entities;

namespace LittleSoftChat.User.Domain.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<List<UserEntity>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<UserEntity>> SearchUsersAsync(string keyword);
    Task<int> CreateAsync(UserEntity user);
    Task<bool> UpdateAsync(UserEntity user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
