using FluentMigrator;

namespace LittleSoftChat.Database.Migration.Migrations;

[Migration(202506080002)]
public class SeedTestData : FluentMigrator.Migration
{
    public override void Up()
    {
        // 插入測試用戶
        Insert.IntoTable("users").Row(new
        {
            username = "admin",
            email = "admin@littlesoftchat.com",
            password_hash = "$2a$11$test.hash.for.admin.user",
            display_name = "Administrator",
            is_active = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        });

        Insert.IntoTable("users").Row(new
        {
            username = "user1",
            email = "user1@example.com",
            password_hash = "$2a$11$test.hash.for.user1",
            display_name = "Test User 1",
            is_active = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        });

        Insert.IntoTable("users").Row(new
        {
            username = "user2",
            email = "user2@example.com",
            password_hash = "$2a$11$test.hash.for.user2",
            display_name = "Test User 2",
            is_active = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").Row(new { username = "admin" });
        Delete.FromTable("users").Row(new { username = "user1" });
        Delete.FromTable("users").Row(new { username = "user2" });
    }
}
