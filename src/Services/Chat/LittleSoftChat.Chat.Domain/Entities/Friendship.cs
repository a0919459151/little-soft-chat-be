namespace LittleSoftChat.Chat.Domain.Entities;

public class FriendshipEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
}

public enum FriendshipStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}
