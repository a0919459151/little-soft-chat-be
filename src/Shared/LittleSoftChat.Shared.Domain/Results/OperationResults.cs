using LittleSoftChat.Shared.Domain.DTOs;

namespace LittleSoftChat.Shared.Domain.Results;

public class SendMessageResult
{
    public bool IsSuccess { get; set; }
    public int MessageId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static SendMessageResult Success(int messageId) => new() { IsSuccess = true, MessageId = messageId };
    public static SendMessageResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public class FriendRequestResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static FriendRequestResult Success() => new() { IsSuccess = true };
    public static FriendRequestResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public class LoginResult
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static LoginResult Success(string token, UserDto user) => new() 
    { 
        IsSuccess = true, 
        Token = token, 
        User = user 
    };
    
    public static LoginResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
