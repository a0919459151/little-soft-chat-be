namespace LittleSoftChat.Notification.Domain.Entities;

public class NotificationConnectionEntity
{
    public string ConnectionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public bool IsActive { get; set; }
}

public class NotificationHistoryEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // "message", "friend_request", "system"
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
