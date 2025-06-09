namespace LittleSoftChat.Shared.Domain.Enums;

/// <summary>
/// 好友關係狀態
/// </summary>
public enum FriendshipStatus
{
    /// <summary>
    /// 待處理
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// 已接受
    /// </summary>
    Accepted = 1,
    
    /// <summary>
    /// 已拒絕
    /// </summary>
    Rejected = 2
}

/// <summary>
/// 訊息類型
/// </summary>
public enum MessageType
{
    /// <summary>
    /// 文字訊息
    /// </summary>
    Text = 0,
    
    /// <summary>
    /// 圖片訊息
    /// </summary>
    Image = 1,
    
    /// <summary>
    /// 檔案訊息
    /// </summary>
    File = 2,
    
    /// <summary>
    /// 系統訊息
    /// </summary>
    System = 3
}

/// <summary>
/// 通知類型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 系統通知
    /// </summary>
    System = 0,
    
    /// <summary>
    /// 訊息通知
    /// </summary>
    Message = 1,
    
    /// <summary>
    /// 好友請求通知
    /// </summary>
    FriendRequest = 2,
    
    /// <summary>
    /// 好友接受通知
    /// </summary>
    FriendAccepted = 3
}
