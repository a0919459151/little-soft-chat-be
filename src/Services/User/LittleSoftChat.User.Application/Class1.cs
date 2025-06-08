using MediatR;
using LittleSoftChat.Shared.Domain.Results;

namespace LittleSoftChat.User.Application.Commands;

// Commands (寫操作)
public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;

public record RegisterCommand(string Username, string Email, string Password, string DisplayName) : IRequest<LoginResult>;

public record UpdateUserProfileCommand(int UserId, string DisplayName, string Avatar) : IRequest<bool>;
