using LittleSoftChat.User.Application;
using LittleSoftChat.User.Infrastructure;
using LittleSoftChat.Shared.Infrastructure;
using LittleSoftChat.User.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add application layers
builder.Services.AddUserApplication();
builder.Services.AddUserInfrastructure();
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

app.UseHttpsRedirection();

// Use shared infrastructure middleware
app.UseSharedInfrastructure();

app.MapGrpcService<UserGrpcService>();

app.MapGet("/", () => "User Service is running!");
app.MapGet("/health", () => "User Service is healthy!");

app.Run();
