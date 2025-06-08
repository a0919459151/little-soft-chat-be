using MediatR;
using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.User.Application.Queries;

// Queries (讀操作)
public record GetUserProfileQuery(int UserId) : IRequest<UserDto?>;

public record GetUserByIdQuery(int UserId) : IRequest<UserDto?>;

public record GetUsersByIdsQuery(List<int> UserIds) : IRequest<List<UserDto>>;

public record SearchUsersQuery(string Keyword) : IRequest<List<UserDto>>;

public record ValidateUserQuery(int UserId) : IRequest<bool>;
