namespace LittleSoftChat.Gateway.API.Models;

public class SendFriendRequestRequest
{
    public int ReceiverId { get; set; }
}

public class FriendResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}

public class FriendRequestResponse
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string SenderFullName { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class FriendRequestResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
