namespace LittleSoftChat.Chat.API.Models;

public class SendMessageRequest
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
}

public class SendFriendRequestRequest
{
    public int ReceiverId { get; set; }
}

public class AcceptFriendRequestRequest
{
    public int FriendshipId { get; set; }
}

public class RejectFriendRequestRequest
{
    public int FriendshipId { get; set; }
}

public class GetMessagesRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}
