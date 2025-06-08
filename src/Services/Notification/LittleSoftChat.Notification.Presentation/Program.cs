using Microsoft.AspNetCore.Authentication.JwtBearer;
using LittleSoftChat.Notification.Application;
using LittleSoftChat.Notification.Infrastructure;
using LittleSoftChat.Notification.Application.Hubs;
using LittleSoftChat.Notification.Presentation.Services;
using LittleSoftChat.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173") // React/Vue app
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for SignalR
    });
});

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Add gRPC
builder.Services.AddGrpc();

// Register application dependencies
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Configure JWT for SignalR after shared infrastructure registration
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Use shared infrastructure middleware
app.UseSharedInfrastructure();

app.MapGrpcService<NotificationGrpcService>();
app.MapHub<ChatHub>("/chatHub");

app.MapGet("/", () => "Notification Service is running!");
app.MapGet("/health", () => "Notification Service is healthy!");

app.Run();


