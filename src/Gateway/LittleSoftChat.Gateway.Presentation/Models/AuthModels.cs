namespace LittleSoftChat.Gateway.Presentation.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public UserProfile? User { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}
