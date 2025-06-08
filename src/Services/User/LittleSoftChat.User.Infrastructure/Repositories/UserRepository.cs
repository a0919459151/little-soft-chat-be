using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using LittleSoftChat.User.Domain.Entities;
using LittleSoftChat.User.Domain.Repositories;
using LittleSoftChat.Shared.Infrastructure.Database;

namespace LittleSoftChat.User.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory dbFactory, ILogger<UserRepository> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }    public async Task<Domain.Entities.UserEntity?> GetByIdAsync(int id)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, username, email, password_hash as PasswordHash, display_name as DisplayName, 
                   avatar, is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            FROM users 
            WHERE id = @Id";

        return await connection.QuerySingleOrDefaultAsync<Domain.Entities.UserEntity>(sql, new { Id = id });
    }    public async Task<Domain.Entities.UserEntity?> GetByUsernameAsync(string username)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, username, email, password_hash as PasswordHash, display_name as DisplayName, 
                   avatar, is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            FROM users 
            WHERE username = @Username";

        return await connection.QuerySingleOrDefaultAsync<Domain.Entities.UserEntity>(sql, new { Username = username });
    }    public async Task<Domain.Entities.UserEntity?> GetByEmailAsync(string email)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, username, email, password_hash as PasswordHash, display_name as DisplayName, 
                   avatar, is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            FROM users 
            WHERE email = @Email";

        return await connection.QuerySingleOrDefaultAsync<Domain.Entities.UserEntity>(sql, new { Email = email });
    }    public async Task<List<Domain.Entities.UserEntity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, username, email, password_hash as PasswordHash, display_name as DisplayName, 
                   avatar, is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            FROM users 
            WHERE id IN @Ids";

        var users = await connection.QueryAsync<Domain.Entities.UserEntity>(sql, new { Ids = ids });
        return users.ToList();
    }    public async Task<List<Domain.Entities.UserEntity>> SearchUsersAsync(string keyword)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, username, email, password_hash as PasswordHash, display_name as DisplayName, 
                   avatar, is_active as IsActive, created_at as CreatedAt, updated_at as UpdatedAt
            FROM users 
            WHERE (username LIKE @Keyword OR display_name LIKE @Keyword) 
            AND is_active = 1
            LIMIT 50";

        var users = await connection.QueryAsync<Domain.Entities.UserEntity>(sql, new { Keyword = $"%{keyword}%" });
        return users.ToList();
    }

    public async Task<int> CreateAsync(Domain.Entities.UserEntity user)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO users (username, email, password_hash, display_name, avatar, is_active, created_at, updated_at)
            VALUES (@Username, @Email, @PasswordHash, @DisplayName, @Avatar, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT LAST_INSERT_ID();";

        var id = await connection.QuerySingleAsync<int>(sql, user);
        _logger.LogInformation("Created user with ID {UserId}", id);
        return id;
    }    public async Task<bool> UpdateAsync(Domain.Entities.UserEntity user)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET display_name = @DisplayName, avatar = @Avatar, updated_at = @UpdatedAt
            WHERE id = @Id";

        user.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, user);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"
            UPDATE users 
            SET is_active = 0, updated_at = @UpdatedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _dbFactory.CreateConnection();
        
        const string sql = @"SELECT COUNT(1) FROM users WHERE id = @Id AND is_active = 1";
        
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
