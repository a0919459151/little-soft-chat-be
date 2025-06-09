using LittleSoftChat.Shared.Domain.Enums;

namespace LittleSoftChat.Shared.Domain.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; } = MessageType.Text;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class FriendDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public UserDto Friend { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class FriendRequestDto
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int ReceiverId { get; set; }
    public UserDto Requester { get; set; } = new();
    public UserDto Receiver { get; set; } = new();
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; } = NotificationType.System;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
