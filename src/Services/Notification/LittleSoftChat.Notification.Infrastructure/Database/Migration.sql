-- 通知歷史表
CREATE TABLE IF NOT EXISTS `NotificationHistory` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `Type` VARCHAR(50) NOT NULL COMMENT 'message, friend_request, system',
    `Title` VARCHAR(255) NOT NULL,
    `Content` TEXT NOT NULL,
    `IsRead` BOOLEAN NOT NULL DEFAULT FALSE,
    `CreatedAt` DATETIME NOT NULL,
    `ReadAt` DATETIME NULL,
    INDEX `IX_NotificationHistory_UserId` (`UserId`),
    INDEX `IX_NotificationHistory_CreatedAt` (`CreatedAt`),
    INDEX `IX_NotificationHistory_IsRead` (`IsRead`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 用戶在線狀態表（可選，如果需要持久化在線狀態）
CREATE TABLE IF NOT EXISTS `UserOnlineStatus` (
    `UserId` INT PRIMARY KEY,
    `IsOnline` BOOLEAN NOT NULL DEFAULT FALSE,
    `LastSeen` DATETIME NOT NULL,
    `UpdatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
