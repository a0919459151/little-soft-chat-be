using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Gateway.API.Models;

public class SendMessageRequest
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
}

public class SendMessageResponse
{
    public bool IsSuccess { get; set; }
    public int MessageId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class GetMessagesRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}

public class MessageResponse
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

public class GetMessagesResponse
{
    public List<MessageResponse> Messages { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}
