using FluentMigrator;

namespace LittleSoftChat.Database.Migration.Migrations;

[Migration(202506080001)]
public class InitialCreate : FluentMigrator.Migration
{
    public override void Up()
    {
        // 用戶表
        Create.Table("users")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("username").AsString(50).NotNullable()
            .WithColumn("email").AsString(100).NotNullable()
            .WithColumn("password_hash").AsString(255).NotNullable()
            .WithColumn("display_name").AsString(100).NotNullable()
            .WithColumn("avatar").AsString(255).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        // 創建用戶表的唯一索引
        Create.UniqueConstraint("uk_users_username").OnTable("users").Column("username");
        Create.UniqueConstraint("uk_users_email").OnTable("users").Column("email");

        // 友誼關係表
        Create.Table("friendships")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("friend_id").AsInt32().NotNullable()
            .WithColumn("status").AsString(20).NotNullable().WithDefaultValue("pending")
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("ix_friendships_user_id").OnTable("friendships").OnColumn("user_id");
        Create.Index("ix_friendships_friend_id").OnTable("friendships").OnColumn("friend_id");
        Create.Index("ix_friendships_status").OnTable("friendships").OnColumn("status");
        Create.UniqueConstraint("uk_friendships_user_friend").OnTable("friendships").Columns("user_id", "friend_id");

        // 消息表
        Create.Table("messages")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("sender_id").AsInt32().NotNullable()
            .WithColumn("receiver_id").AsInt32().NotNullable()
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("message_type").AsString(20).NotNullable().WithDefaultValue("text")
            .WithColumn("is_read").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("read_at").AsDateTime().Nullable();

        Create.Index("ix_messages_sender_id").OnTable("messages").OnColumn("sender_id");
        Create.Index("ix_messages_receiver_id").OnTable("messages").OnColumn("receiver_id");
        Create.Index("ix_messages_created_at").OnTable("messages").OnColumn("created_at");
        Create.Index("ix_messages_is_read").OnTable("messages").OnColumn("is_read");

        // 通知歷史表
        Create.Table("notification_history")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("type").AsString(50).NotNullable()
            .WithColumn("title").AsString(255).NotNullable()
            .WithColumn("content").AsString(int.MaxValue).NotNullable()
            .WithColumn("is_read").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("read_at").AsDateTime().Nullable();

        Create.Index("ix_notification_history_user_id").OnTable("notification_history").OnColumn("user_id");
        Create.Index("ix_notification_history_created_at").OnTable("notification_history").OnColumn("created_at");
        Create.Index("ix_notification_history_is_read").OnTable("notification_history").OnColumn("is_read");

        // 用戶在線狀態表
        Create.Table("user_online_status")
            .WithColumn("user_id").AsInt32().PrimaryKey()
            .WithColumn("is_online").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("last_seen").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("user_online_status");
        Delete.Table("notification_history");
        Delete.Table("messages");
        Delete.Table("friendships");
        Delete.Table("users");
    }
}
