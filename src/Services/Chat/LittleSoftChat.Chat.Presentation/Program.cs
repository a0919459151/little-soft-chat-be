using LittleSoftChat.Chat.Application;
using LittleSoftChat.Chat.Infrastructure;
using LittleSoftChat.Chat.Presentation.Services;
using LittleSoftChat.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC
builder.Services.AddGrpc();

// Register application dependencies
builder.Services.AddChatApplication();
builder.Services.AddChatInfrastructure();
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

// Use shared infrastructure middleware (includes validation exception handling)
app.UseSharedInfrastructure();

app.MapGrpcService<ChatGrpcService>();

app.MapGet("/", () => "Chat Service is running!");
app.MapGet("/health", () => "Chat Service is healthy!");

app.Run();
